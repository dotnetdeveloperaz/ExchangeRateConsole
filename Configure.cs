using Microsoft.Extensions.Configuration;

namespace ExchangeRateConsole;

public class Configure
{
    public static IConfiguration ConfigureAppSettings()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddUserSecrets<Program>()
            .Build();

        return configBuilder;
    }
}