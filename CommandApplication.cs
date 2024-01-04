using ExchangeRateConsole.Commands;
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
                .AddCommand<RestoreCacheCommand>("restore")
                .WithDescription("Restores Cache From Failed Or Saved Completion");
            config
                .AddCommand<RateCommand>("rate")
                .WithDescription("Get Exchange Rates")
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
                .AddCommand<AccountCommand>("account")
                .WithAlias("acct")
                .WithDescription("Gets Account Statistics.")
                .WithExample(new[] { "account", "--debug", "--hidden", "--appid", "<AppId>" });

            config
                .AddCommand<TestDatabaseCommand>("testdb")
                .WithDescription("Tests The Database Configuration.")
                .WithExample(new[] { "testdb", "--debug", "--hidden" });
            config
                .AddCommand<SymbolCommand>("symbol")
                .WithDescription("List Or Search Currency Symbols")
                .WithExample(new[] { "symbol", "--search", "<USD>,<EUR>", "--list" });
#if DEBUG
            config.PropagateExceptions();
            config.ValidateExamples();
#endif
        });
        return app;
    }
}
