using System.Reflection;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Commands.Settings;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands
{
    public class CacheStatsCommand : AsyncCommand<CacheStatsCommand.Settings>
    {
        private readonly ApiServer _apiServer;
        public CacheStatsCommand(ApiServer apiServer)
        {
            _apiServer = apiServer;
        }

        public class Settings : BaseCommandSettings
        {

        }
        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            settings.CacheFile ??= _apiServer.CacheFile;
            int rowSize = _apiServer.CacheFileExists ? 13 : 11;
            if (settings.Debug)
            {
                if (!DebugDisplay.Print(settings, _apiServer, "N/A"))
                    return 0;
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
                    Update(70, () => table.AddRow($"[red bold]Status[/] [green bold]Checking For Cache File[/]"));
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string file = Path.Combine(path, settings.CacheFile);
                    // Content

                    if (!File.Exists(file))

                    {
                        Update(70, () => table.AddRow($"[red]No Cache File Exists ({file}). Exiting.[/]"));
                        return;
                    }

                    Update(70, () => table.AddRow($"[yellow]Loading [/][green]{file}[/]"));
                    string cache;
 
                    using (StreamReader sr = new StreamReader(file))
                    {
                        cache = sr.ReadToEnd();
                    }
                    List<Exchange> exchangeRate = await JsonSerializer.DeserializeAsync<List<Exchange>>(new MemoryStream(Encoding.UTF8.GetBytes(cache)));
                    Update(70, () => table.AddRow($"[yellow]Cache File Loaded[/] [green]{exchangeRate.Count} Days Loaded[/]"));
                    foreach (Exchange exchange in exchangeRate)
                    {
                        var rates = exchange.rates;
                        int cnt = 0;
                        string symbols = "";
                        foreach (PropertyInfo prop in rates.GetType().GetProperties())
                        {
                            if (prop.GetValue(rates).ToString() != "0")
                            {
                                cnt++;
                                if (cnt <=5 )
                                {
                                    symbols += prop.Name + " ";
                                }
                                if (cnt > 5)
                                    symbols += "and more....";
                            }
                        // More rows than we want?
                            if (table.Rows.Count > Console.WindowHeight - rowSize)
                            {
                                // Remove the first one
                                table.Rows.RemoveAt(0);
                            }
                        } 
                        Update(70, () => table.AddRow($"  [green]Rates for {cnt} Currency Symbols {symbols}for {exchange.RateDate.ToString("MM/dd/yyyy")}[/]"));  
                    }  
                    Update(70, () => table.Columns[0].Footer("[blue]Complete[/]"));
                });
            return 0;
        }
    }
}