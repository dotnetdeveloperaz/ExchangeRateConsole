using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

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
        if (settings.Debug)
        {
            if (!DebugDisplay.Print(settings, _config, _connectionString, url))
                return 0;

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
                    exchange = JsonSerializer.Deserialize<Exchange>(cache);
                }
                else
                {
                    exchange = await Utility.GetExchangeRateAsync(url, settings.Save, _connectionString);
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
