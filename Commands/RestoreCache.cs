using ExchangeRateConsole.Models;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Reflection;

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

                if (settings.Debug)
                {
                    if (settings.ShowHidden)
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]WebAPI AppId: [/][blue]{_config.AppId.Replace("?app_id=", "")}[/]"
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
                        () => titleTable.AddRow($"[red bold]Debug: [/][blue]{settings.Debug}[/]")
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Secret: [/][blue]{settings.ShowHidden}[/]"
                            )
                    );
                }

                Update(70, () =>
                    titleTable.AddRow(
                        $"[red bold]Loading Cache File...[/]"));
                string cache = File.ReadAllText("ExchangeRate.cache");
                var exchanges = JsonConvert.DeserializeObject<List<Exchange>>(cache);
                Update(70, () =>
                            titleTable.AddRow(
                                "[green bold]Cache File Loaded[/]"));
                foreach (var exchange in exchanges)
                {
                    Utility.SaveRate(exchange, _connectionString);
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
                Update(70, () =>
                            titleTable.AddRow(
                                "[red bold]Cleaning Up Cache[/]"));
                File.Delete("ExchangeRate.cache");

                Update(70, () =>
                            titleTable.AddRow(
                                "[green bold]Cache Cleared[/]"));
                Update(
                    70,
                    () =>
                        titleTable.Columns[0].Footer(
                            $"[green bold]Restore Process Complete[/]"
                        )
                );
            });
        return 0;
    }
}
