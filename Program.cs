﻿using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

class Program
{
    public static async Task Main(string[] args)
    {
        Title.Print();
        var config = Configure.ConfigureAppSettings();
        IServiceCollection serviceCollection = ConfigureServices(config);
        var registrar = new TypeRegistrar(serviceCollection);

        var app = new CommandApp(registrar);
        app.Configure(configure => CommandApplication.Initialize(app));


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
        var logging = config.GetSection("Logging");
        var database = config.GetSection("ConnectionStrings");
        config = config.GetSection("ApiServer");
        string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string file = Path.Combine(path, config["CacheFile"]);
        var services = new ServiceCollection();
        services.AddSingleton(new ApiServer() 
        { 
            AppId = config.GetSection("AppId").Value, 
            BaseSymbol = config.GetSection("BaseSymbol").Value,
            BaseUrl = config.GetSection("BaseUrl").Value, 
            CacheFile = config.GetSection("CacheFile").Value, 
            CacheFileExists = File.Exists(file), 
            History = config.GetSection("History").Value, 
            Latest = config.GetSection("Latest").Value,
            MaxViewCount = config.GetSection("MaxViewCount").Value,
            HistoricalStartDate = config.GetSection("HistoricalStartDate").Value,
            Usage = config.GetSection("Usage").Value 
        });
        services.AddSingleton(new ConnectionStrings() { DefaultDB = database["DefaultDB"] });
        services.AddLogging(loggingBuilder =>
      {
          loggingBuilder.AddConfiguration(config.GetSection("Logging"));
          loggingBuilder.AddEventSourceLogger();
      });

        services.AddSingleton<ExchangeRateEventSource>();
        services.BuildServiceProvider();
        return services;
    }
}
