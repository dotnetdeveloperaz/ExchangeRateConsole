using System;
using Newtonsoft.Json;

namespace ExchangeRateConsole
{
    public class Account
    {
        public int status { get; set; }
        public Data data { get; set; }
    }
    public class Features
    {
        public bool @base { get; set; }
        public bool symbols { get; set; }
        public bool experimental { get; set; }

        [JsonProperty("time-series")]
        public bool TimeSeries { get; set; }
        public bool convert { get; set; }
    }
    public class Plan
    {
        public string name { get; set; }
        public string quota { get; set; }
        public string update_frequency { get; set; }
        public Features features { get; set; }
    }
    public class Usage
    {
        public int requests { get; set; }
        public int requests_quota { get; set; }
        public int requests_remaining { get; set; }
        public int days_elapsed { get; set; }
        public int days_remaining { get; set; }
        public int daily_average { get; set; }
    }
    public class Data
    {
        public string app_id { get; set; }
        public string status { get; set; }
        public Plan plan { get; set; }
        public Usage usage { get; set; }
    }
}