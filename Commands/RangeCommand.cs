using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Globalization;
using ExchangeRateConsole.Commands.Settings;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class RangeCommand : AsyncCommand<RangeCommand.Settings>
{
    private string _defaultDB;
    private readonly ConnectionStrings _connectionStrings;
    private readonly ApiServer _apiServer;

    public RangeCommand(ApiServer apiServer, ConnectionStrings connectionStrings)
    {
        _apiServer = apiServer;
        _defaultDB = connectionStrings.DefaultDB;
        _connectionStrings = connectionStrings;

    }

    public class Settings : RateCommandSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, RangeCommand.Settings settings)
    {
        var startDate = DateTime.Parse(settings.StartDate);
        var endDate = DateTime.Parse(settings.EndDate);
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
                var url =
                    _apiServer.BaseUrl
                    + _apiServer.History
                    + "?app_id=" + _apiServer.AppId
                    + "&symbols="
                    + settings.Symbols
                    + "&base="
                    + settings.BaseSymbol;
                Update(70, () => titleTable.AddRow($"[red bold]Calculating Number Of Days[/]"));
                var numDays = Utility.GetNumberOfDays(settings.StartDate, settings.EndDate);
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $":check_mark:[green bold] {numDays.Count} Days To Process...[/]"
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
                    // This is just currently pulling all from sample file since it's small so all parameters are ignored.
                    exchanges = JsonSerializer.Deserialize<List<Exchange>>(cache);
                }
                else
                {
                    exchanges = await Utility.GetExchangeRatesAsync(
                        url,
                        settings.StartDate,
                        settings.EndDate,
                        settings.Save,
                        _defaultDB
                    );
                }
                Update(
                    70,
                    () => titleTable.AddRow($"[green bold] Retrieved {exchanges.Count} Rate(s) Using Base Currency {exchanges[0].@base}...[/]")
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
