using Microsoft.Extensions.Configuration;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

class Configure
{
    public static Configuration Load(Configuration configuration)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets<Program>()
            .Build();

        // Need to change this to pull from ConnectionStrings
        if (config.GetSection("DefaultDB").Value != "")
            configuration.DefaultDB = config.GetSection("DefaultDB").Value;
        if (config.GetSection("AppId").Value != "")
            configuration.AppId = config.GetSection("AppId").Value;

        configuration.BaseURL = config.GetSection("BaseURL").Value;
        configuration.DefaultSymbols = config.GetSection("DefaultSymbols").Value;
        configuration.History = config.GetSection("History").Value;
        configuration.Latest = config.GetSection("Latest").Value;
        configuration.Usage = config.GetSection("Usage").Value;
        configuration.BaseSymbol = config.GetSection("BaseSymbol").Value;
        return configuration;
    }
}
