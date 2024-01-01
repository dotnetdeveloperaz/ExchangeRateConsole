using ExchangeRateConsole.Models;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace ExchangeRateConsole.Commands;

public class GetRateCommand : AsyncCommand<GetRateCommand.Settings>
{
    private readonly string _connectionString;
    private readonly ApiServer _config;
    private ExchangeRateEventSource _eventSource;

    public GetRateCommand(ApiServer config, ConnectionStrings ConnectionString, ExchangeRateEventSource eventSource)
    {
        _config = config;
        _connectionString = ConnectionString.DefaultDB;
        _eventSource = eventSource;
    }

    public class Settings : RateCommandSettings
    {
        [Description("Get Rate For Specified Date")]
        [DefaultValue(false)]
        public bool GetRate { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        bool notToday = false;
        settings.GetRate = true;
        if (settings.StartDate == null)
            settings.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
        else
            notToday = true;
        bool skip = Utility.IsHolidayOrWeekend(settings.StartDate);
        var url = _config.BaseUrl;
        if (notToday)
        {
            url += _config.History
            + "?app_id=" + _config.AppId
            + "&symbols="
            + settings.Symbols
            + "&base="
            + settings.BaseSymbol;
            url = url.Replace("{date}", settings.StartDate);
        }
        else
        { 
            url += _config.Latest
            + "?app_id=" + _config.AppId
            + "&symbols="
            + settings.Symbols
            + "&base="
            + settings.BaseSymbol;
        }
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

        // Animate
        await AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .StartAsync(async ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }

                if (settings.Debug)
                {
                    if (settings.ShowHidden)
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Calling WebAPI URL: [/][blue]{url}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]WebAPI AppId: [/][blue]{_config.AppId}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Database Connection: [/][blue]{_connectionString}[/]"
                                )
                        );
                    }
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Url: [/][blue]{_config.BaseUrl}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]History Url: [/][blue]{_config.History}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Latest Url: [/][blue]{_config.Latest}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Usage Url: [/][blue]{_config.Usage}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Symbol: [/][blue]{_config.BaseSymbol}[/]"
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
                                $"[red bold]Date To Get Rate(s): [/][blue]{settings.StartDate}[/]"
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
                        () => titleTable.AddRow($"[red bold]Holiday Or Weekend: [/][blue]{skip}[/]")
                    );
                }
                if (skip)
                {
                    Update(
                        70,
                        () => titleTable.Columns[0].Footer($":stop_sign: [red bold]Skipping Holiday Or Weekend: [/][blue]{settings.StartDate}[/]")
                    );
                    return;
                }
                Update(
                    70,
                    () => titleTable.AddRow($"[red bold]Retrieving Exchange Rates For {settings.StartDate}[/]")
                );
                Exchange exchange;
                if (settings.IsFake)
                {
                    string cache = File.ReadAllText("OneDayRate.sample");
                    exchange = JsonConvert.DeserializeObject<Exchange>(cache);
                }
                else
                {
                    exchange = Utility.GetExchangeRate(url, settings.Save, _connectionString);
                }
                var rates = exchange.rates;
                Update(
                    70,
                    () => titleTable.AddRow($":check_mark:[green bold] Retrieved Rate(s) Using Base Currency {exchange.@base}...[/]")
                );

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
                                        $":check_mark:[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
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
                                            $":check_mark:[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
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
