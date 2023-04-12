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
        Title.Print();
        configuration = Configure.Load(configuration);
        var app = CommandApplication.Initialize();
        try
        {
            await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
}
