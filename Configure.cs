using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

public class Configure
{
    public static IConfigurationRoot ConfigureAppSettings()
    {
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return configBuilder.Build();
    }
}