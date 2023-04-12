using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

class Program
{
    static Configuration configuration = new();
    static Exchange exchange = new();

    public static async Task Main(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(
            builder => builder
//                .AddConsole();
//                .AddDebug()
//                .SetMinimumLevel(LogLevel.Debug)
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("ExchangeRateConsole.Program", LogLevel.Debug)
        );

        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogInformation("Exchange Rate Console Starting Up");
        Title.Print();
        configuration = Configure.Load(configuration);
        var app = CommandApplication.Initialize();
        try
        {
            await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            logger.LogError($"Spectre.Console.App.RunAsync threw an error. {ex.Message}");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
}
