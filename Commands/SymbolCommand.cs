using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using System.Text;
using Spectre.Console.Cli;
using Spectre.Console;
using ExchangeRateConsole.Models;
using ExchangeRateConsole.Commands.Settings;

namespace ExchangeRateConsole.Commands
{
    public class SymbolCommand : AsyncCommand<SymbolCommand.Settings>
    {
        private readonly string _connectionString;
        private readonly ApiServer _apiServer;

        public SymbolCommand(ApiServer apiServer, ConnectionStrings ConnectionString)
        {
            _apiServer = apiServer;
            _connectionString = ConnectionString.DefaultDB;
        }
        public class Settings : BaseCommandSettings
        {
            [Description("List or Search Symbols")]
            [DefaultValue(false)]
            public bool DoSymbols { get; set; }

            [CommandOption("--list")]
            [Description("List All Currency Symbols")]
            [DefaultValue(false)]
            public bool ListCodes { get; set; }

            [CommandOption("--search <USD>")]
            [Description("Search For A Currency Code")]
            [DefaultValue("")]
            public string Symbol { get; set; }
        }

        public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
        {
            settings.DoSymbols = true;
            if (settings.Debug)
            {
                if (!DebugDisplay.Print(settings, _apiServer, _connectionString, "N/A"))
                    return 0;

            }
            var table = new Table().Centered();
            // Borders
            table.BorderColor(Color.Blue);
            table.MinimalBorder();
            table.SimpleBorder();
            table.AddColumn(
                new TableColumn(
                    new Markup(
                        "[yellow bold]Currency Code Search/List[/]"
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
                    
                    string msg = settings.Symbol == String.Empty ? "Listing Valid Currency Codes" : $"Searching For Currency Codes Containing {settings.Symbol}";
                    Update(70, () => table.AddRow($"[red bold]{msg}[/]"));
                    string cache;
                    using (StreamReader sr = new StreamReader("OneDayRate.sample"))
                    {
                        cache = sr.ReadToEnd();
                    }
                    List<Exchange> exchange = await JsonSerializer.DeserializeAsync<List<Exchange>>(new MemoryStream(Encoding.UTF8.GetBytes(cache)));
                    var rates = exchange[0].rates;

                    foreach (PropertyInfo prop in rates.GetType().GetProperties())
                    {
                        if (settings.ListCodes || prop.Name.Contains(settings.Symbol))
                            Update(70, () => table.AddRow($"[green bold] {prop.Name}[/]"));
                        // More rows than we want?
                        if (table.Rows.Count > Console.WindowHeight - 15)
                        {
                            // Remove the first one
                            table.Rows.RemoveAt(0);
                        }
                    }

                    Update(70, () => table.Columns[0].Footer("[green bold]Finished....[/]"));
                });
            return 0;
        }
    }
}
