using System;
using System.ComponentModel;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;
using Newtonsoft.Json;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class GetRateCommand : Command<GetRateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Rate For Specified Date")]
        [DefaultValue(false)]
        public bool GetRate { get; set; }

        [CommandOption("--date <YYYY-MM-DD>")]
        [Description("Date To Get Rate(s) For")]
        public string Date { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        [DefaultValue("USD")]
        public string BaseSymbol { get; set; }

        [CommandOption("--symbols <EUR>")]
        [Description("Exchange Rate(s) To Get")]
        public string Symbols { get; set; }

        [CommandOption("--save")]
        [Description("Save Results")]
        [DefaultValue(false)]
        public bool Save { get; set; }

        [CommandOption("--json")]
        [Description("Display The Raw JSON Response")]
        [DefaultValue(false)]
        public bool DisplayJson { get; set; }

        [CommandOption("--pretty")]
        [Description("Display The Raw JSON In Friendly Format Instead Of Minified")]
        [DefaultValue(false)]
        public bool Pretty { get; set; }

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
        settings.GetRate = true;
        if (settings.Date == null)
            settings.Date = DateTime.Now.ToString("yyyy-MM-dd");
        var url =
            Configure.Configuration.BaseURL
            + Configure.Configuration.Latest
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
        titleTable.AddColumn(
            new TableColumn(
                new Markup(
                    $"[yellow bold]Retrieving Exchange Rates For {settings.Symbols}[/]"
                ).Centered()
            )
        );
        titleTable.BorderColor(Color.Blue);
        titleTable.Border(TableBorder.Rounded);
        titleTable.Expand();
        var client = new HttpClient();
        var response = client.GetAsync(url).GetAwaiter().GetResult();
        var results = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        // Animate
        AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
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
                    + Configure.Configuration.Latest
                    + Configure.Configuration.AppId
                    + "&symbols="
                    + settings.Symbols
                    + "&base="
                    + settings.BaseSymbol;
                Update(
                    70,
                    () => titleTable.AddRow($"[red bold]Calling Full URL: [/][blue]{url}[/]")
                );
                var client = new HttpClient();
                var response = client
                    .GetAsync(
                        Configure.Configuration.BaseURL
                            + Configure.Configuration.Latest
                            + settings.Symbols
                            + Configure.Configuration.AppId
                    )
                    .GetAwaiter()
                    .GetResult();
                var info = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (settings.DisplayJson)
                {
                    Update(70, () => titleTable.AddRow($"[red]Data: {info}[/]"));
                }
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
                                $"[red bold]Base Symbol: [/][blue]{Configure.Configuration.BaseSymbol}[/]"
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
                                $"[red bold]Date To Get Rate(s): [/][blue]{settings.Date}[/]"
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
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Json: [/][blue]{settings.DisplayJson}[/]"
                            )
                    );
                    Update(
                        70,
                        () => titleTable.AddRow($"[red bold]Pretty: [/][blue]{settings.Pretty}[/]")
                    );
                }
                var exchange = JsonConvert.DeserializeObject<Exchange>(info);
                var rates = exchange.rates;
                Update(
                    70,
                    () => titleTable.AddRow($":check_mark:[green bold] Retrieved Rate(s)...[/]")
                );

                foreach (PropertyInfo prop in rates.GetType().GetProperties())
                {
                    if (prop.GetValue(rates).ToString() != "0")
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $":check_mark:[green bold] {prop.Name}    {double.Parse(prop.GetValue(rates).ToString())} - {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2)}...[/]"
                                )
                            );
                        if (settings.Save)
                        {
//                            await SaveAsync(prop.Name, double.Parse(prop.GetValue(rates).ToString()), exchangeRate.RateDate.ToString("yyyy-MM-dd"));
                        }
                    }
                }
            });
        return 0;
    }
}
