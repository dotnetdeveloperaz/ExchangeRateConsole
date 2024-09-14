using ExchangeRateConsole.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole;

public class CommandApplication
{
    public static CommandApp Initialize(CommandApp app)
    {
        // Configure Spectre Cli
        //var app = new CommandApp();
        app.Configure(config =>
        {
            config
                .AddCommand<RangeCommand>("range")
                .WithDescription(
                    "Gets historical Exchange rate(s). Use --save to save to the database.\r\nWeekends and holidays are skipped because markets are closed."
                )
                .WithExample(
                    new[]
                    {
                        "rate",
                        "--start",
                        "YYYY-MM-DD",
                        "--end",
                        "YYYY-MM-DD",
                        "--symbols",
                        "EUR,TRY",
                        "--base",
                        "USD",
                        "--appid",
                        "<AppID>",
                        "--debug",
                        "--hidden",
                        "--save",
                        "--cache",
                        "--fake",
                        "--appid",
                        "<AppId>"
                    }
                );

            config
                .AddCommand<RateCommand>("rate")
                .WithDescription(
                    "Gets the current Exchange rate(s). Use --save to save to database. Weekends and holidays are skipped."
                )
                .WithExample(
                    new[] 
                    { 
                        "rate", 
                        "--symbols", 
                        "EUR,TRY", 
                        "--base", 
                        "USD", 
                        "--date", 
                        "YYYY-MM-DD", 
                        "--save", 
                        "--appid", 
                        "AppID", 
                        "--debug", 
                        "--hidden" 
                    }
                );

            config
                .AddCommand<ViewCommand>("view")
                .WithDescription(
                    "Works like the Range command except it displays data from "
                    + "the configured database or from the configured cachefile with --cache. "
                    + "Use with --cachefile </path/filename to override.\n"
                )
                .WithExample(
                    new[]
                    {
                            "view",
                            "--start",
                            "YYYY-MM-DD",
                            "--end",
                            "YYYY-MM-DD",
                            "--symbols",
                            "EUR,TRY",
                            "--base",
                            "USD",
                            "--fake",
                            "--cache",
                            "--cachefile",
                            "<file>",
                            "--debug",
                            "--appid",
                            "<AppID>"
                    }
                 );

            config
                .AddCommand<CacheStatsCommand>("cachestats")
                .WithAlias("cstats")
                .WithDescription(
                    "Displays the cachefile statistics, start and end dates.\n"
                    + "To override configured cache file, use the --cachefile </path/filename> switch.\n"
                    )
                .WithExample(new[] { "cachestats", "--cachefile", "<filename>" });

            config
                .AddCommand<TestDatabaseCommand>("testdb")
                .WithDescription(
                    "Tests the configured database connection.\n"
                    + "Use the --db \"<YourConnectionString>\" (Quotes Required!) to test connectionstrings for diagnosing.\n"
                    + "This switch is NOT available with any other command.\n"
                    )
                .WithExample(new[] { "testdb", "--db", "'<YourDBConnectionString>'", "--debug", "--hidden" });

            config
                .AddCommand<RestoreCacheCommand>("restore")
                .WithDescription("Writes data from cache file to database and deletes the cache file after successful completion.");

            config
                .AddCommand<AccountCommand>("account")
                .WithAlias("acct")
                .WithDescription("Gets account information.")
                .WithExample(new[] { "acct", "--debug", "--hidden" });

            config
                .AddCommand<StatusCommand>("status")
                .WithDescription("Gets WebApi Status.")
                .WithExample(new[] { "status", "--debug", "--hidden" });

#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
        });
        return app;
    }
}
