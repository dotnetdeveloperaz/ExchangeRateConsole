using System.Text.Json.Serialization;

namespace ExchangeRateConsole
{
    public class Account
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("data")]
        public Data Data { get; set; }
    }
    public class Features
    {
        [JsonPropertyName("@base")]
        public bool @Base { get; set; }
        [JsonPropertyName("symbols")]
        public bool Symbols { get; set; }
        [JsonPropertyName("experimental")]
        public bool Experimental { get; set; }
        [JsonPropertyName("time-series")]
        public bool TimeSeries { get; set; }
        [JsonPropertyName("convert")]
        public bool Convert { get; set; }
    }
    public class Plan
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("quota")]
        public string Quota { get; set; }
        [JsonPropertyName("update_frequency")]
        public string UpdateFrequency { get; set; }
        [JsonPropertyName("features")]
        public Features Features { get; set; }
    }
    public class Usage
    {
        [JsonPropertyName("requests")]
        public int Requests { get; set; }
        [JsonPropertyName("requests_quota")]
        public int RequestsQuota { get; set; }
        [JsonPropertyName("requests_remaining")]
        public int RequestsRemaining { get; set; }
        [JsonPropertyName("days_elapsed")]
        public int DaysElapsed { get; set; }
        [JsonPropertyName("days_remaining")]
        public int DaysRemaining { get; set; }
        [JsonPropertyName("daily_average")]
        public int DailyAverage { get; set; }
    }
    public class Data
    {
        [JsonPropertyName("app_id")]
        public string AppId { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("plan")]
        public Plan Plan { get; set; }
        [JsonPropertyName("usage")]
        public Usage Usage { get; set; }
    }
}