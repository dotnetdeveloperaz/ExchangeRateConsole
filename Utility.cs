using System.Data;
using System.Reflection;
using System.Text.Json;
using MySqlConnector;
using PublicHoliday;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

public class Utility
{
    public static List<DateTime> GetNumberOfDays(string startDate, string endDate)
    {
        List<DateTime> dates = new();
        DateTime start = DateTime.Parse(startDate);
        DateTime end = DateTime.Parse(endDate);

        while (start <= end)
        {
            if (!IsHolidayOrWeekend(start))
                dates.Add(start);
            start = start.AddDays(1);
        }
        return dates;
    }

    public static bool IsHolidayOrWeekend(DateTime date)
    {
        bool isHoliday = new USAPublicHoliday().IsPublicHoliday(date);
        if (isHoliday || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;
        else
            return false;
    }

    public static async Task<Exchange> GetExchangeRateAsync(string Uri, bool Save, string ConnectionString)
    {
        var client = new HttpClient();
        var response = await client.GetAsync(Uri);
        var info = await response.Content.ReadAsStringAsync();
        var exchangeRate = JsonSerializer.Deserialize<Exchange>(info);

        if (Save)
            await SaveRateAsync(exchangeRate, ConnectionString);

        return exchangeRate;
    }

    public static async Task<List<Exchange>> GetExchangeRatesAsync(
        string Uri,
        string StartDate,
        string EndDate,
        bool Save,
        string ConnectionString
    )
    {
        DateTime startDate = DateTime.Parse(StartDate);
        DateTime endDate = DateTime.Parse(EndDate);

        List<Exchange> exchangeRates = new List<Exchange>();
        while (startDate <= endDate)
        {
            var url = Uri.Replace("{date}", startDate.ToString("yyyy-MM-dd"));
            if(!IsHolidayOrWeekend(startDate))
                exchangeRates.Add(await GetExchangeRateAsync(url, Save, ConnectionString));
            startDate = startDate.AddDays(1);
        }

        return exchangeRates;
    }

    public static async Task<bool> SaveRateAsync(Exchange ExchangeRate, string ConnectionString)
    {
        var rates = ExchangeRate.rates;

        foreach (PropertyInfo prop in rates.GetType().GetProperties())
        {
            if (prop.GetValue(rates).ToString() != "0")
            {
                var Symbol = prop.Name;
                var BaseSymbol = ExchangeRate.@base;
                var Rate = double.Parse(prop.GetValue(rates).ToString());
                var RateDate = ExchangeRate.RateDate.ToString("yyyy-MM-dd");

                MySqlConnection sqlConnection = new MySqlConnection(ConnectionString);
                MySqlCommand sqlCommand = new MySqlCommand("usp_AddExchangeRate", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                try
                {
                    await sqlConnection.OpenAsync();
                    sqlCommand.Parameters.AddWithValue("symbol", Symbol);
                    sqlCommand.Parameters.AddWithValue("baseSymbol", BaseSymbol);
                    sqlCommand.Parameters.AddWithValue("rate", Rate);
                    sqlCommand.Parameters.AddWithValue("ratedate", RateDate);
                    var recs = await sqlCommand.ExecuteNonQueryAsync();
                }
                catch (MySqlException)
                {
                    return false;
                }
                finally
                {
                    if (sqlConnection.State == ConnectionState.Open)
                        sqlConnection.Close();
                    sqlCommand.Dispose();
                    sqlConnection.Dispose();
                }
            }
        }
        return true;
    }
    public static bool CacheData(List<Exchange> exchanges, string cacheFile)
    {
        string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string file = Path.Combine(path, cacheFile);
        if (File.Exists(file))
        {
            var json = File.ReadAllText(file);
            List<Exchange> cache = JsonSerializer.Deserialize<List<Exchange>>(json);
            foreach (var exchange in exchanges)
                cache.Add(exchange);
            string result = JsonSerializer.Serialize(cache);
            File.WriteAllText(file, result);
        }
        else
        {
            string result = JsonSerializer.Serialize(exchanges);
            File.WriteAllText(file, result);
        }
        return true;
    }

}
