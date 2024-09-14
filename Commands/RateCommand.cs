using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

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
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.GetRate = true;
        var url = _config.BaseUrl;

        url += _config.History;
        if (settings.StartDate == String.Empty)
            settings.StartDate = DateTime.Now.ToString("yyyy-MM-dd");

        url += "?symbols="
            + settings.Symbols;

        if (settings.OverrideAppId == String.Empty)
            url += $"&app_id={_config.AppId}";
        else
            url += $"&app_id={settings.OverrideAppId}";

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

                Update(
                        70,
                        () =>
                            table.AddRow(
                                $"[red bold]Status[/] [green bold]Calculating Non-Weekend/Holiday Dates For {settings.StartDate} to {settings.EndDate}[/]"
                            )
                    );

                List<DateTime> days = Utility.GetNumberOfDays(settings.StartDate, settings.EndDate);

                string msg = days.Count == 1 ? $"[green bold]There is {days.Count} Day To Retrieve Rates For...[/]" : $"[green bold]There are {days.Count} Days To Retrieve Rates For...[/]";
                Update(70, () => table.AddRow(msg));

                if (days.Count < 1)
                {
                    Update(70, () => table.Columns[0].Footer(
                                $"[red bold]Status[/] [green bold]Completed, Nothing Processed. Holiday Or Weekend.[/]"
                            ));
                    return;
                }


                Update(70, () => table.Columns[0].Footer($"[green]Calling {_config.BaseUrl}stat For Account Information[/]"));

                msg = settings.EndDate == null ? $"Retrieving Exchanges Rate For {settings.StartDate}" : $"Retrieving Exchange Rate for {settings.StartDate} to {settings.EndDate}";
                string sampleFile = settings.EndDate == null ? "OneDayRate.sample" : "MultiDayRate.sample";
                Update(
                    70,
                    () => table.Columns[0].Header($"[green bold]{msg}[/]")
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
                    foreach (var day in days)
                    {
                        var callingUri = url.Replace("{date}", day.ToString("yyyy-MM-dd"));
                        Update(70, () => table.Columns[0].Footer($"[green]Calling {callingUri}[/]"));
                        Exchange exchange = await Utility.GetExchangeRateAsync(callingUri, settings.Save, _connectionString);
                        exchanges.Add(exchange);
                    }
                }
                if (settings.Cache)
                {
                    Update(
                       70,
                       () => table.AddRow($"[red bold]     Caching Data[/]")
                   );
                    Utility.CacheData(exchanges, "ExchangeRate.cache");
                }
                foreach (Exchange exchange in exchanges)
                { 
                    var rates = exchange.rates;
                    Update(
                        70,
                        () => table.AddRow($"[red bold]     Retrieved Rate(s) For {exchange.RateDate:yyyy-MM-dd} Using Base Currency {exchange.@base}...[/]")
                    );

                    foreach (PropertyInfo prop in rates.GetType().GetProperties())
                    {
                        if (prop.GetValue(rates).ToString() != "0")
                        {
                            double price = 1 / double.Parse(prop.GetValue(rates).ToString());
                            if (!settings.IsFake)
                            {
                                Update(
                                    70,
                                () =>
                                table.AddRow(
                                            $"[green bold]     {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("00.00")}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}      USD Cost {price.ToString("C")}[/]"
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
                                                $"[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}      USD Cost {price.ToString("C")}[/]"
                                            ));
                                }
                                else
                                    Update(70, () =>
                                        table.AddRow(
                                            $"[green bold] {prop.Name}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("C", CultureInfo.CurrentCulture)}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")}      USD Cost {price.ToString("C")}[/]"
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

    public override ValidationResult Validate(CommandContext context, Settings settings)
    {
        if (settings.StartDate == String.Empty)
            settings.StartDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (!DateTime.TryParse(settings.StartDate, out _))
            return ValidationResult.Error($"Invalid date - {settings.StartDate}");
        if (settings.EndDate == String.Empty)
            settings.EndDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (!DateTime.TryParse(settings.EndDate, out _))
            return ValidationResult.Error($"Invalid date - {settings.EndDate}");
        return base.Validate(context, settings);
    }
}