using System.ComponentModel;
using System.Reflection;
using Spectre.Console;
using Spectre.Console.Cli;
using MySqlConnector;
using Newtonsoft.Json;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class RestoreCacheCommand : Command<RestoreCacheCommand.Settings>
{
    private readonly Configuration _config;
    private ExchangeRateEventSource _eventSource;

    public RestoreCacheCommand(Configuration config, ExchangeRateEventSource eventSource)
    {
        _config = config;
        _eventSource = eventSource;
    }

    public class Settings : CommandSettings
    {
        [Description("Restore Cache.")]
        [DefaultValue(false)]
        public bool RestoreCache { get; set; }

        [CommandOption("--debug")]
        [Description("Enable Debug Output")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [CommandOption("--hidden")]
        [Description("Enable Secret Debug Output")]
        [DefaultValue(false)]
        public bool ShowHidden { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
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
        AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
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
                                    $"[red bold]Database Connection: [/][blue]{_config.ConnectionStrings.DefaultDB}[/]"
                                )
                        );
                    }
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Url: [/][blue]{_config.BaseURL}[/]"
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
                        $":hourglass_not_done:[red bold] Loading Cache File...[/]"));
                string cache = File.ReadAllText("ExchangeRate.cache");
                var exchanges = JsonConvert.DeserializeObject<List<Exchange>>(cache);
                Update(70, () =>
                            titleTable.AddRow(
                                ":check_mark:[green bold] Cache File Loaded[/]"));
                foreach (var exchange in exchanges)
                {
                    Utility.SaveRate(exchange, _config.ConnectionStrings.DefaultDB);
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
                                            $":check_mark:[green bold] {prop.Name}      {date}      {Math.Round(double.Parse(prop.GetValue(rates).ToString()), 2).ToString("00000.00")}      {double.Parse(prop.GetValue(rates).ToString()).ToString("00000.000000")}[/]"
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
                                ":hourglass_not_done:[red bold] Cleaning Up Cache[/]"));
                File.Delete("ExchangeRate.cache");

                Update(70, () =>
                            titleTable.AddRow(
                                ":check_mark:[green bold] Cache Cleared[/]"));
                Update(
                    70,
                    () =>
                        titleTable.Columns[0].Footer(
                            $":check_mark:[green bold] Restore Process Complete[/]"
                        )
                );
            });
        return 0;
    }
}
