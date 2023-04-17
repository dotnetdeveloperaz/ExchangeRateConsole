using Newtonsoft.Json;

namespace ExchangeRateConsole.Models;

public class Configuration
{
    public string AppId { get; set; }
    public string BaseURL { get; set; }
    public string DefaultSymbols { get; set; }
    public string BaseSymbol { get; set; }
    public string History { get; set; }
    public string Latest { get; set; }
    public string Usage { get; set; }
    public Logging Logging { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public string AllowedHosts { get; set; }
}

public class ConnectionStrings
{
    public string DefaultDB { get; set; }
}

public class Logging
{
    public LogLevel LogLevel { get; set; }
}

public class LogLevel
{
    public string Default { get; set; }
    public string Microsoft { get; set; }

    [JsonPropertyAttribute("Microsoft.Hosting.Lifetime")]
    public string MicrosoftHostingLifetime { get; set; }
}
