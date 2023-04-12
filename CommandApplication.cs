using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Commands;

namespace ExchangeRateConsole;

public class CommandApplication
{
    public static CommandApp Initialize()
    {
        // Configure Spectre Cli
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.ValidateExamples();
            config
                .AddCommand<AccountCommand>("account")
                .WithDescription("Gets account information.");

            config
                .AddCommand<RangeCommand>("range")
                .WithDescription(
                    "Gets historical Exchange rate(s). Use --save to save to the database.\r\nWeekends and holidays are skipped because markets are closed."
                )
                .WithExample(
                    new[]
                    {
                        "range",
                        "--start",
                        "YYYY-MM-DD",
                        "--end",
                        "YYYY-MM-DD",
                        "--json", 
                        "--pretty", 
                        "--debug",
                        "--hidden"
                    }
                );

            config
                .AddCommand<HistoryCommand>("history")
                .WithDescription(
                    "Gets the current Exchange rate(s). Use --save to save to database. Weekends and holidays are skipped."
                )
                .WithExample(
                    new[] { "history", "--date", "YYYY-MM-DD", "--save", "--json", "--pretty", "--debug", "--hidden" }
                );

            config
                .AddCommand<AccountCommand>("acct")
                .WithDescription("Gets Account Statistics.")
                .WithExample(new[] { "acct", "--json", "--pretty", "--debug", "--hidden" });

            config
                .AddCommand<StatusCommand>("status")
                .WithDescription("Gets WebApi Status.")
                .WithExample(new[] { "status", "--json", "--pretty", "--debug", "--hidden" });

            config
                .AddCommand<TestDatabaseCommand>("testdb")
                .WithDescription("Tests The Database Connection.")
                .WithExample(new[] { "testdb", "--debug", "--hidden" });

            config.PropagateExceptions();
        });
        return app;
    }
}
