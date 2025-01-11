using System.ComponentModel;
using System.Data;
using MySqlConnector;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;
using ExchangeRateConsole.Commands.Settings;

namespace ExchangeRateConsole.Commands;

public class TestDatabaseCommand : AsyncCommand<TestDatabaseCommand.Settings>
{
    private string _defaultDB;
    private readonly ConnectionStrings _connectionStrings;
    private readonly ApiServer _apiServer;

    public TestDatabaseCommand(ApiServer apiServer, ConnectionStrings connectionStrings)
    {
        _apiServer = apiServer;
        _connectionStrings = connectionStrings;
        _defaultDB = connectionStrings.DefaultDB;
    }

    public class Settings : BaseCommandSettings
    {
        [CommandOption("--db <ConnectionString>")]
        [Description("Override Configured DB For Testing")]
        [DefaultValue(null)]
        public string DBConnectionString { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (settings.Debug)
        {
            if (!DebugDisplay.Print(settings, _apiServer, _defaultDB, "N/A"))
                return 0;

        }
        // Database connection string is being overridden, we displayed DEBUG info (if selected), now update the connectionstring used.
        // Setting this BEFORE DebugDisplay would cause both to have the same values, so it must come after.
        settings.DBConnectionString = settings.DBConnectionString ?? _defaultDB;
        var titleTable = new Table().Centered();
        // Borders
        titleTable.BorderColor(Color.Blue);
        titleTable.MinimalBorder();
        titleTable.SimpleBorder();
        titleTable.AddColumn(
            new TableColumn(
                new Markup(
                    "[yellow bold]Running Database Configuration Test[/]"
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

                Update(70, () => titleTable.AddRow($"[blue bold]Testing Connection...[/]"));
                var conn = new MySqlConnection(settings.DBConnectionString);
                var sqlCommand = new MySqlCommand();
                var csb = new MySqlConnectionStringBuilder(settings.DBConnectionString);
                var user = "'" + csb.UserID + "'" + "@" + "'" + csb.Server + "'";
                var schema = csb.Database;
                sqlCommand.Connection = conn;
                sqlCommand.CommandType = CommandType.Text;
                try
                {
                    await conn.OpenAsync();
                    Update(70, () => titleTable.AddRow($"[green bold]Connection Made Successfully...[/]"));

                    // Want to add tests to ensure table exists, and both stored procedures exist.
                    string procs = "SELECT COUNT(ROUTINE_NAME) FROM information_schema.ROUTINES WHERE ROUTINE_NAME = 'usp_AddExchangeRate' "
                        + "OR ROUTINE_NAME LIKE 'usp_GetExchangeRates%';";
                    string table = "SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_NAME = 'ExchangeRates';";
                    string exec = $"select COUNT(*) from information_schema.schema_Privileges where TABLE_SCHEMA = '{schema}' AND GRANTEE = \"" + user + "\" and PRIVILEGE_TYPE = 'EXECUTE';";

                    Update(70, () => titleTable.AddRow($"[blue bold]Verifying Table Exists...[/]"));
                    sqlCommand.CommandText = table;
                    var recs = sqlCommand.ExecuteScalar();
                    if (recs.ToString() == "1")
                        Update(70, () => titleTable.AddRow($"[green bold]Verified Table Exists....[/]"));
                    else
                        Update(70, () => titleTable.AddRow($"[red bold]Table DOES NOT Exists....[/]"));

                    Update(70, () => titleTable.AddRow($"[blue bold]Verifying The 3 Stored Procedures Exist...[/]"));
                    sqlCommand.CommandText = procs;
                    recs = sqlCommand.ExecuteScalar();

                    if (recs.ToString() == "3")
                        Update(70, () => titleTable.AddRow($"[green bold]Verified {recs} Stored Procedures Exist...[/]"));
                    else
                        Update(70, () => titleTable.AddRow($"[red bold]The THREE Stored Procedures DO NOT Exists, Count {recs}....[/]"));

                    Update(70, () => titleTable.AddRow($"[blue bold]Verifying User {user} Has Execute Permissions....[/]"));
                    sqlCommand.CommandText = exec;
                    recs = sqlCommand.ExecuteScalar();

                    if(recs.ToString() == "1")
                        Update(70, () => titleTable.AddRow($"[green bold]Verified User {user} Has Execute Permissions...[/]"));
                    else
                        Update(70, () => titleTable.AddRow($"[red bold]The User {user} Does NOT have EXECUTE Permissions, Count {recs}....[/]"));

                }
                catch (Exception ex)
                {
                    Update(70, () => titleTable.AddRow($"[red bold]Error Connecting to Database: {ex.Message}[/]"));
                }
                finally
                {
                    Update(70, () => titleTable.AddRow($"[yellow bold]Cleaning up...[/]"));
                    if (conn.State ==
                    ConnectionState.Open)
                        await conn.CloseAsync();
                    await conn.DisposeAsync();
                }

                Update(70, () => titleTable.AddRow("[blue bold]Database Connection Test Complete[/]"));
            });
        return 0;
    }

    /*
        public override ValidationResult Validate(CommandContext context, BaseCommandSettings settings)
        {
            return base.Validate(context, settings);
        }
    */
}

