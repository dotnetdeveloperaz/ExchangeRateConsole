using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using MySqlConnector;

namespace ExchangeRateConsole.Commands;

public class TestDatabaseCommand : Command<TestDatabaseCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Web API Status.")]
        [DefaultValue(false)]
        public bool DoTestDatabase { get; set; }

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
                    "[yellow bold]Running Database Connection Configuration Test[/]"
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

                settings.DoTestDatabase = true;
                Update(70, () =>
                    titleTable.AddRow(
                        $":hourglass_not_done:[red bold] Testing Connection...[/]"));
                var conn = new MySqlConnection(Configure.Configuration.DefaultDB);
                try
                {
                    conn.Open();
                }
                catch(Exception ex)
                {
                Update(70, () =>
                            titleTable.AddRow(
                                $"[red bold]Error Connecting to Database: {ex.Message}"));
                }
                conn.Close();
                Update(70, () =>
                            titleTable.AddRow(
                                ":check_mark:[green bold] Connection Successful[/]"));
            });
        return 0;
    }
}
