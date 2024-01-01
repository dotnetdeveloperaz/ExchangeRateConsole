using ExchangeRateConsole.Models;
using MySqlConnector;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace ExchangeRateConsole.Commands;

public class TestDatabaseCommand : AsyncCommand<TestDatabaseCommand.Settings>
{
    private readonly string _connectionString;
    //private ILogger eventSource { get; }

    //    public TestDatabaseCommand(IConfigurationSection config, ILogger<Program> eventSource)
    public TestDatabaseCommand(ConnectionStrings ConnectionString)
    {
        _connectionString = ConnectionString.DefaultDB;
        // _ = eventSource ?? throw new ArgumentNullException(nameof(eventSource));
    }

    public class Settings : BaseCommandSettings
    {
        [Description("Test Database Configuration.")]
        [DefaultValue(false)]
        public bool DoTestDatabase { get; set; }
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
                    "[yellow bold]Running Database Connection Configuration Test[/]"
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

                settings.DoTestDatabase = true;
                Update(70, () =>
                    titleTable.AddRow(
                        $"[red bold] Testing Connection...[/]"));
                var conn = new MySqlConnection(_connectionString);
                try
                {
                    await conn.OpenAsync();
                }
                catch (Exception ex)
                {
                    Update(70, () =>
                                titleTable.AddRow(
                                    $"[red bold]Error Connecting to Database: {ex.Message}[/]"));
                }
                conn.Close();
                Update(70, () =>
                            titleTable.AddRow(
                                "[green bold] Connection Successful[/]"));
            });
        return 0;
    }
}
