using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class RestoreCacheCommand : AsyncCommand<RestoreCacheCommand.Settings>
{
    private readonly ApiServer _config;
    private readonly string _connectionString;
    private ExchangeRateEventSource _eventSource;

    public RestoreCacheCommand(ApiServer config, ConnectionStrings ConnectionString, ExchangeRateEventSource eventSource)
    {
        _config = config;
        _connectionString = ConnectionString.DefaultDB;
        _eventSource = eventSource;
    }

    public class Settings : BaseCommandSettings
    {
        [Description("Restore Cache.")]
        [DefaultValue(false)]
        public bool RestoreCache { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.RestoreCache = true;
        if (settings.Debug)
        {
            if (!DebugDisplay.Print(settings, _config, _connectionString, "N/A"))
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
                    "[yellow bold]Running Restore Cache[/]"
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

                settings.RestoreCache = true;

                Update(70, () => titleTable.AddRow($"[red bold]Status[/] [green bold]Checking For Cache File[/]"));
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string file = Path.Combine(path, _config.CacheFile);
                // Content
                if (!File.Exists(file))
                {
                    Update(70, () => titleTable.AddRow($"[red]No Cache File Exists ({file}). Exiting.[/]"));
                    return;
                }

                Update(70, () => titleTable.AddRow($"[red bold]Loading Cache File...[/]"));
                string cache;
                using (StreamReader sr = new StreamReader(file))
                {
                    cache = sr.ReadToEnd();
                }
                List<Exchange> exchanges = await JsonSerializer.DeserializeAsync<List<Exchange>>(new MemoryStream(Encoding.UTF8.GetBytes(cache)));
                Update(70, () => titleTable.AddRow("[green bold]Cache File Loaded[/]"));

                foreach (var exchange in exchanges)
                {
                    await Utility.SaveRateAsync(exchange, _connectionString);
                    var rates = exchange.rates;
                    var date = exchange.RateDate.ToString("MM-dd-yyyy");

                    foreach (PropertyInfo prop in rates.GetType().GetProperties())
                    {
                        if (prop.GetValue(rates).ToString() != "0")
                        {
                            Update(
                                70,
                                () =>
                                    titleTable.AddRow(
                                            $"[green bold]{prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("00000.00")}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00000.000000")}[/]"
                                    )
                            );
                        }
                        // More rows than we want?
                        if (titleTable.Rows.Count > Console.WindowHeight - 15)
                        {
                            // Remove the first one
                            titleTable.Rows.RemoveAt(0);
                        }
                    }
                }
                Update(70, () => titleTable.AddRow("[red bold]Cleaning Up Cache[/]"));
                File.Delete(file);

                Update(70, () => titleTable.AddRow("[green bold]Cache Cleared[/]"));
                Update(70, () => titleTable.Columns[0].Footer($"[green bold]Restore Process Complete[/]"));
            });
        return 0;
    }
}
