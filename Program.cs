using Microsoft.Extensions.Logging;
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
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("ExchangeRateConsole.Program", LogLevel.Debug);
        });

        ILogger logger = loggerFactory.CreateLogger<Program>();
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
            logger.Error($"Spectre.Console.App.RunAsync threw an error. {ex.Message}");
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return;
        }
    }
}
