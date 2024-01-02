using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;
using System.Globalization;

namespace ExchangeRateConsole.Commands;

public class RateCommand : AsyncCommand<RateCommand.Settings>
{
    private readonly string _connectionString;
    private readonly ApiServer _config;
    public RateCommand(ApiServer config, ConnectionStrings ConnectionString)
    {
        _config = config;
        _connectionString = ConnectionString.DefaultDB;
    }

    public class Settings : RateCommandSettings
    {
        [Description("Get Rate For Specified Date")]
        [DefaultValue(false)]
        public bool GetRate { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.GetRate = true;
        var url = _config.BaseUrl;

        url += settings.StartDate == null ? _config.Latest : _config.History;
        if (settings.StartDate == null)
            settings.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
        url = url.Replace("{date}", settings.StartDate) + "?app_id=" 
            + _config.AppId
            + "&symbols="
            + settings.Symbols
            + "&base="
            + settings.BaseSymbol;

        if (settings.Debug)
        {
            if (!DebugDisplay.Print(settings, _config, _connectionString, url))
                return 0;
        }
        string symbols = settings.Symbols == null ? "All Symbols" : settings.Symbols;
        var table = new Table().Centered();
        // Borders
        table.BorderColor(Color.Blue);
        table.MinimalBorder();
        table.SimpleBorder();
        table.AddColumn(
            new TableColumn(
                new Markup(
                    $"[yellow bold]Retrieving Exchange Rates For {symbols}[/]"
                ).Centered()
            )
        );
        table.BorderColor(Color.Blue);
        table.Border(TableBorder.Rounded);
        table.Expand();

        // Animate
        await AnsiConsole
            .Live(table)
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

                string msg = settings.EndDate == null ? $"Retrieving Exchanges Rate For {settings.StartDate}" : $"Retrieving Exchange Rate for {settings.StartDate} to {settings.EndDate}";
                string sampleFile = settings.EndDate == null ? "OneDayRate.sample" : "MultiDayRate.sample";
                Update(
                    70,
                    () => table.AddRow($"[red bold]{msg}[/]")
                );
                List<Exchange> exchanges = new();
                if (settings.IsFake)
                {
                    string cache = File.ReadAllText(sampleFile);
                    if (sampleFile.Contains("OneDay"))
                    {
                        Exchange exchange = JsonSerializer.Deserialize<Exchange>(cache);
                        exchanges.Add(exchange);
                    }
                    else
                    {
                        exchanges = JsonSerializer.Deserialize<List<Exchange>>(cache);
                    }
                }
                else
                {
                    Exchange exchange = await Utility.GetExchangeRateAsync(url, settings.Save, _connectionString);
                    exchanges.Add(exchange);
                }
                foreach (Exchange exchange in exchanges)
                { 
                    var rates = exchange.rates;
                    Update(
                        70,
                        () => table.AddRow($"[red bold] Retrieved Rate(s) For {exchange.RateDate} Using Base Currency {exchange.@base}...[/]")
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
                                table.AddRow(
                                            $"[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
                                        )
                                );
                            }
                            else
                            {
                                if (settings.Symbols != null)
                                {
                                    if (settings.Symbols.Contains(prop.Name))
                                        Update(70, () =>
                                            table.AddRow(
                                                $"[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
                                            ));
                                }
                                else
                                    Update(70, () =>
                                        table.AddRow(
                                            $"[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}[/]"
                                        ));
                            }
                        }
                        // More rows than we want?
                        if (table.Rows.Count > Console.WindowHeight - 10)
                        {
                            // Remove the first one
                            table.Rows.RemoveAt(0);
                        }
                    }
                }
                Update(
                    70,
                    () =>
                        table.Columns[0].Footer(
                            $"[green bold] Process Complete[/]"
                        )
                );
            });
        return 0;
    }
}