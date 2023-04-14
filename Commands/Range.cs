using System.ComponentModel;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;
using Newtonsoft.Json;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class RangeCommand : Command<RangeCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Rates For A Date Range.")]
        [DefaultValue(false)]
        public bool GetRange { get; set; }

        [CommandOption("--start <startdate>")]
        [Description("Start Date.")]
        public string StartDate { get; set; }

        [CommandOption("--end <enddate>")]
        [Description("End Date")]
        public string EndDate { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        [DefaultValue("USD")]
        public string BaseSymbol { get; set; }

        [CommandOption("--symbols <Symbols>")]
        [Description("Symbol(s) To Get Rate For")]
        [DefaultValue("")]
        public string Symbols { get; set; }

        [CommandOption("--save")]
        [Description("Save Results")]
        [DefaultValue(false)]
        public bool Save { get; set; }

        [CommandOption("--fake")]
        [Description("Displays Fake Data Instead Of Calling WebAPI")]
        [DefaultValue(false)]
        public bool IsFake { get; set; }

        [CommandOption("--debug")]
        [Description("Enable Debug Output")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [CommandOption("--hidden")]
        [Description("Enable Secret Debug Output")]
        [DefaultValue(false)]
        public bool ShowHidden { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        var startDate = DateTime.Parse(settings.StartDate);
        var endDate = DateTime.Parse(settings.EndDate);
        settings.GetRange = true;
        var url =
            Configure.Configuration.BaseURL
            + Configure.Configuration.History.Replace("{date}", settings.StartDate)
            + Configure.Configuration.AppId
            + "&symbols="
            + settings.Symbols
            + "&base="
            + settings.BaseSymbol;
        var titleTable = new Table().Centered();
        // Borders
        titleTable.BorderColor(Color.Blue);
        titleTable.MinimalBorder();
        titleTable.SimpleBorder();
        var symbols = settings.Symbols;
        if (settings.Symbols == "")
            symbols = "All Currencies";

        titleTable.AddColumn(
            new TableColumn(
                new Markup($"[yellow bold]Retrieving Exchange Rates For {symbols}[/]").Centered()
            )
        );
        titleTable.BorderColor(Color.Blue);
        titleTable.Border(TableBorder.Rounded);
        titleTable.Expand();
        // Animate
        AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .Start(ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }
                var url =
                    Configure.Configuration.BaseURL
                    + Configure.Configuration.History
                    + Configure.Configuration.AppId
                    + "&symbols="
                    + settings.Symbols
                    + "&base="
                    + settings.BaseSymbol;
                Update(70, () => titleTable.AddRow($"[red bold]Calculating Number Of Days[/]"));
                var numDays = Utility.GetNumberOfDays(startDate, endDate);
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $":check_mark:[green bold] {numDays} Days To Process...[/]"
                        )
                );
                if (settings.Debug)
                {
                    if (settings.ShowHidden)
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Calling WebAPI URL: [/][blue]{Configure.Configuration.BaseURL}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]WebAPI AppId: [/][blue]{Configure.Configuration.AppId.Replace("?app_id=", "")}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Database Connection: [/][blue]{Configure.Configuration.DefaultDB}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow($"[red bold]Calling Full URL: [/][blue]{url}[/]")
                        );
                    }
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Url: [/][blue]{Configure.Configuration.BaseURL}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]History Url: [/][blue]{Configure.Configuration.History}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Latest Url: [/][blue]{Configure.Configuration.Latest}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Usage Url: [/][blue]{Configure.Configuration.Usage}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Symbol: [/][blue]{settings.BaseSymbol}[/]"
                            )
                    );
                    Update(
                        70,
                        () => titleTable.AddRow($"[red bold]Debug: [/][blue]{settings.Debug}[/]")
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Start Date To Get Rate(s) From: [/][blue]{settings.StartDate}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]End Date To Get Rate(s) From: [/][blue]{settings.EndDate}[/]"
                            )
                    );
                    Update(
                        70,
                        () => titleTable.AddRow($"[red bold]Save: [/][blue]{settings.Save}[/]")
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Secret: [/][blue]{settings.ShowHidden}[/]"
                            )
                    );
                }
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[red bold]Retrieving Rates from [/][blue]{settings.StartDate}[/] [red bold] To [/][blue]{settings.EndDate}[/]"
                        )
                );
                List<Exchange> exchanges = new List<Exchange>();
                if (settings.IsFake)
                {
                    string cache = File.ReadAllText("MultiDayRate.sample");
                    exchanges = JsonConvert.DeserializeObject<List<Exchange>>(cache);
                }
                else
                {
                    exchanges = Utility.GetExchangeRates(
                        url,
                        settings.StartDate,
                        settings.EndDate,
                        settings.Save
                    );
                }
                Update(
                    70,
                    () => titleTable.AddRow($":check_mark:[green bold] Retrieved Rate(s)...[/]")
                );

                foreach (var exchange in exchanges)
                {
                    var rates = exchange.rates;
                    var date = exchange.RateDate.ToString("MM-dd-yyyy");
                    foreach (PropertyInfo prop in rates.GetType().GetProperties())
                    {
                        if (prop.GetValue(rates).ToString() != "0")
                        {
                            if (!settings.IsFake)
                            {
                                Update(
                                    70,
                                    () =>
                                        titleTable.AddRow(
                                            $":check_mark:[green bold] {prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("00.00")}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
                                        )
                                );
                            }
                            else
                            {
                                if (settings.Symbols.Contains(prop.Name))
                                    Update(
                                        70,
                                        () =>
                                            titleTable.AddRow(
                                                $":check_mark:[green bold] {prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("00.00")}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
                                            )
                                    );
                            }
                        }
                        // More rows than we want?
                        if (titleTable.Rows.Count > Console.WindowHeight - 15)
                        {
                            // Remove the first one
                            titleTable.Rows.RemoveAt(0);
                        }
                    }
                }
                Update(
                    70,
                    () =>
                        titleTable.Columns[0].Footer(
                            $":check_mark:[green bold] Process Complete[/]"
                        )
                );
            });
        return 0;
    }
}
