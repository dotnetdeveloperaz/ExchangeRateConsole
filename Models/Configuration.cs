using System.Text.Json.Serialization;

namespace ExchangeRateConsole.Models;

public class Configuration
{
    public ApiServer ApiServer { get; set; }
    public Logging Logging { get; set; }
    public ConnectionStrings ConnectionStrings { get; set; }
    public string AllowedHosts { get; set; }
}

public class ApiServer
{
    public string BaseUrl { get; set; }
    public string AppId { get; set; }
    public string BaseSymbol { get; set; }
    public string History { get; set; }
    public string Latest { get; set; }
    public string Usage { get; set; }
    public string CacheFile { get; set; }
    public bool CacheFileExists {get; set; }
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

    [JsonPropertyName("Microsoft.Hosting.Lifetime")]
    public string MicrosoftHostingLifetime { get; set; }
}
