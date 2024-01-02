using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class RangeCommand : AsyncCommand<RangeCommand.Settings>
{
    private readonly string _connectionString;
    private readonly ApiServer _config;
    private ExchangeRateEventSource _eventSource;

    public RangeCommand(ApiServer config, ConnectionStrings ConnectionString, ExchangeRateEventSource eventSource)
    {
        _config = config;
        _connectionString = ConnectionString.DefaultDB;
        _eventSource = eventSource;
    }

    public class Settings : RateCommandSettings
    {
        [Description("Get Rates For A Date Range.")]
        [DefaultValue(false)]
        public bool GetRange { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var startDate = DateTime.Parse(settings.StartDate);
        var endDate = DateTime.Parse(settings.EndDate);
        settings.GetRange = true;
                var url =
                    _config.BaseUrl
                    + _config.History
                    + "?app_id=" + _config.AppId
                    + "&symbols="
                    + settings.Symbols
                    + "&base="
                    + settings.BaseSymbol;
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
        await AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }
                Update(70, () => titleTable.AddRow($"[red bold]Calculating Number Of Days[/]"));
                var numDays = Utility.GetNumberOfDays(startDate, endDate);
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $":check_mark:[green bold] {numDays} Days To Process...[/]"
                        )
                );

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
                    exchanges = JsonSerializer.Deserialize<List<Exchange>>(cache);
                }
                else
                {
                    exchanges = await Utility.GetExchangeRatesAsync(
                        url,
                        settings.StartDate,
                        settings.EndDate,
                        settings.Save,
                        _connectionString
                    );
                }
                Update(
                    70,
                    () => titleTable.AddRow($"[green bold] Retrieved Rate(s) Using Base Currency {exchanges[0].@base}...[/]")
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
                                            $"[green bold] {prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
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
                                                $":[green bold] {prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
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
