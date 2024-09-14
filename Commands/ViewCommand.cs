using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ExchangeRateConsole;
using ExchangeRateConsole.Commands.Settings;
using ExchangeRateConsole.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class ViewCommand : AsyncCommand<ViewCommand.Settings>
{
    private readonly ConnectionStrings _connectionStrings;
    private readonly ApiServer _apiServer;
    private string _defaultDB;

    public ViewCommand(ApiServer apiServer, ConnectionStrings connectionStrings)
    {
        _apiServer = apiServer;
        _connectionStrings = connectionStrings;
    }

    public class Settings : RateCommandSettings
    {
        // There are no special settings for this command

    }
    public override async Task<int> ExecuteAsync(CommandContext context, ViewCommand.Settings settings)
    {
/*
        if (await base.ExecuteAsync(context, settings) == 1)
            return 1;
*/
        _defaultDB = _connectionStrings.DefaultDB;

        // Default to show all data for any date
        DateTime startDate = settings.StartDate == string.Empty ? DateTime.MinValue : DateTime.Parse(settings.StartDate);
        // Ensure we're setting for the latest time for the date, so add a date which will be midnight and subtract 1 second.
        // We could just leave the date after adding a day and the filter would work, but we display the date as well which
        // would show a day later than what the user specifies.
        DateTime endDate = settings.EndDate == string.Empty ? DateTime.MaxValue : DateTime.Parse(settings.EndDate).AddDays(1).AddSeconds(-1);
        int removeSize = 10;
        if (_apiServer.CacheFileExists)
            removeSize += 2;
        // Prompting if date range was not specified or is more than 180 days, although still much I think.
        if (startDate.AddDays(180) < endDate)
        {
            removeSize += 2;
            if (!AnsiConsole.Confirm("[red bold italic]The amount of data that could be returned might be too much.\n" 
                + "You might want to specify a shorter date range. Are you sure?[/] Continue?", false)
                )
                return 1;
        }

        // Process Window
        var table = new Table().Centered();
        table.HideHeaders();
        table.BorderColor(Color.Yellow);
        table.Border(TableBorder.Rounded);
        table.AddColumns(new[] { "" });
        table.Expand();

        // Animate
        await AnsiConsole
            .Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(async ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }
                // Content
                List<ExchangeRate> exchanges = null;
                if (settings.Cache)
                {
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string file = settings.CacheFile ??= Path.Combine(path, settings.CacheFile);

                    Update(70, () => table.AddRow($"[red bold]Status[/] [green bold]Checking For Cache File[/]"));
                    if (!File.Exists(file))
                    {
                        Update(70, () => table.AddRow($"[red]No Cache File Exists ({file}). Exiting.[/]"));
                        return;
                    }

                    Update(70, () => table.AddRow($"[yellow]Loading Cache File From [/][green]{file}[/]"));
                    string cache;
                    using (StreamReader sr = new StreamReader(file))
                    {
                        cache = sr.ReadToEnd();
                    }
                    List<Exchange> tmpPrices = await JsonSerializer.DeserializeAsync<List<Exchange>>(new MemoryStream(Encoding.UTF8.GetBytes(cache)));

                    //exchanges = tmpPrices
                    //.Where(x => x.@base == settings.BaseSymbol && x.RateDate >= startDate && x.RateDate <= endDate).ToList();

                    Update(70, () => table.AddRow($"[yellow]Cache File Loaded[/] [green]{exchanges.Count} of {tmpPrices.Count} Records For Base Currency {settings.BaseSymbol}[/]"));
                    Update(70, () => table.Columns[0].Footer("[blue]Cache File Loaded[/]"));
                }
                else
                {
                    Update(70, () => table.AddRow($"[blue bold]Retrieving Exchange Rates Data From Database From {settings.StartDate} to {settings.EndDate}[/]"));
                    exchanges = await Utility.GetExchangeRates(settings.StartDate, settings.EndDate, settings.Symbols, settings.BaseSymbol, _defaultDB);
                    Update(70, () => table.AddRow($"[green bold]Retrieved {exchanges.Count} Exchange Rates[/]"));
                }
                if (exchanges == null)
                {
                    Update(70, () => table.AddRow($"[red bold]No Rows Of Data Returned[/]"));
                    return;
                }
                Update(70, () => table.AddRow($"[green bold]Finished Retrieving {exchanges.Count} Rows Of Data[/]"));
                int rowCnt = 0;

                foreach (ExchangeRate exchange in exchanges)
                {
                    rowCnt++;
                    Update(
                        70,
                        () =>
                            table.AddRow(
                                $"  [red]{exchange.RateDate} {exchange.Symbol} {exchange.Rate}[/]"
                            )
                    );
/*
                    var rates = exchange.rates;
                    foreach (PropertyInfo prop in rates.GetType().GetProperties())
                    {
                        if (prop.GetValue(rates).ToString() != "0")
                        {
                            Update(
                                70,
                            () =>
                                table.AddRow(
                                    $"    [green bold] {prop.Name}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00.000000")} = 1 {exchange.@base}[/]"
                                )
                            );
                        }
                        // More rows than we want?
                        if (table.Rows.Count > Console.WindowHeight - 8)
                        {
                            // Remove the first one
                            table.Rows.RemoveAt(0);
                        }
                    }
*/
                    // More rows than we can display on screen?
                    if (table.Rows.Count > Console.WindowHeight - removeSize)
                    {
                        table.Rows.RemoveAt(0);
                        table.Rows.RemoveAt(0);
                    }
                }

                Update(70, () => table.Columns[0].Footer($"[blue]Complete. Displayed {rowCnt} of {exchanges.Count} Days Rates[/]"));
            });
        return 0;
    }
    /*
    public override ValidationResult Validate(CommandContext context, RateCommandSettings settings)
    {
        return base.Validate(context, settings);
    }
    */
}