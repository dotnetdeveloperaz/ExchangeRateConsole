using ExchangeRateConsole.Commands;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

class Program
{
    public static async Task Main(string[] args)
    {
        Title.Print();
        var config = Configure.ConfigureAppSettings();
        var serviceCollection = ConfigureServices(config);

        //var app = CommandApplication.Initialize();
        var registrar = new TypeRegistrar(serviceCollection);
        var app = new CommandApp<TestDatabaseCommand>(registrar);

        try
        {
            await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }

    public static IServiceCollection ConfigureServices(IConfiguration config)
    {
        var services = new ServiceCollection();
        services.AddSingleton(config);
/*        services.AddSingleton<Configuration>(
            config.GetSection(nameof(Configuration)).Get<Configuration>()
        );
*/
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConfiguration(config.GetSection("Logging"));
            loggingBuilder.AddEventSourceLogger();
        });

        services.AddSingleton<ExchangeRateEventSource>();
        //services.BuildServiceProvider();
        return services;
    }
}
