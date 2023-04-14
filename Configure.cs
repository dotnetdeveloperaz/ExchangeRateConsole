using Microsoft.Extensions.Configuration;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

public class Configure
{
    private static Configuration _configuration;
    public static Configuration Configuration { get { return _configuration; } }
    public static Version Version { get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; } }

    public static Configuration Load(Configuration configuration)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .AddUserSecrets("b378d9cd-2817-4513-a75f-1e91724e3131")
            .Build();
        var secret = config["TestDB"];
        if (config.GetSection("TestDB").Value != "")
            configuration.DefaultDB = config.GetSection("TestDB").Value;
        if (config.GetSection("AppId").Value != "")
            configuration.AppId = "?app_id=" + config.GetSection("AppId").Value;
        configuration.BaseURL = config.GetSection("BaseURL").Value;
        configuration.DefaultSymbols = config.GetSection("DefaultSymbols").Value;
        configuration.History = config.GetSection("History").Value;
        configuration.Latest = config.GetSection("Latest").Value;
        configuration.Usage = config.GetSection("Usage").Value;
        configuration.BaseSymbol = config.GetSection("BaseSymbol").Value;
        _configuration = configuration;
        return configuration;
    }
}
