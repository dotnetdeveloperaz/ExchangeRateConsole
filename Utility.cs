using System.Data;
using System.Reflection;
using System.Text.Json;
using MySqlConnector;
using PublicHoliday;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

/// <summary>
/// Contains Common Methods
/// </summary>
public class Utility
{
    /// <summary>
    /// Get the number of days between a date range that are non-holiday weekdays.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Returns if a date is a weekend or holiday.
    /// </summary>
    /// <param name="date"></param>
    /// <returns></returns>
    public static bool IsHolidayOrWeekend(DateTime date)
    {
        bool isHoliday = new USAPublicHoliday().IsPublicHoliday(date);
        if (isHoliday || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;
        else
            return false;
    }
    /// <summary>
    /// Retrieves the exchange rates from the third part API.
    /// </summary>
    /// <param name="Uri"></param>
    /// <param name="Save"></param>
    /// <param name="ConnectionString"></param>
    /// <returns></returns>
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
/// <summary>
/// Retrieves the exchange rates from the database
/// </summary>
/// <param name="startDate"></param>
/// <param name="endDate"></param>
/// <param name="symbols"></param>
/// <param name="baseSymbol"></param>
/// <param name="connectionString"></param>
/// <returns></returns>
    public static List<ExchangeRate> GetExchangeRates(string startDate, string endDate, string symbols, string baseSymbol, string connectionString)
    {
        List<ExchangeRate> exchanges = [];
        MySqlConnection sqlConnection = new(connectionString);
        MySqlCommand sqlCommand = new("usp_GetExchangeRates", sqlConnection);
        sqlCommand.CommandType = CommandType.StoredProcedure;
        try
        {
            sqlConnection.Open();
            sqlCommand.Parameters.AddWithValue("startDate", startDate);
            sqlCommand.Parameters.AddWithValue("endDate", endDate);
            sqlCommand.Parameters.AddWithValue("symbols", symbols);
            sqlCommand.Parameters.AddWithValue("baseCurrency", baseSymbol);
            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                exchanges.Add(new ExchangeRate
                {
                    Symbol = reader["Symbol"].ToString(),
                    Rate = double.Parse(reader["Rate"].ToString()),
                    RateDate = DateTime.Parse(reader["RateDate"].ToString()),
                });
            }
            reader.Close();
            reader.Dispose();
        }
        catch (Exception ex)
        {
            using (StreamWriter sw = new("error.log"))
                sw.WriteLine(ex.Message);
        }
        finally
        {
            sqlConnection.Close();
            sqlCommand.Dispose();
            sqlConnection.Dispose();
        }
        return exchanges;
    }
    /// <summary>
    /// Saves the exchange rate to the database.
    /// </summary>
    /// <param name="ExchangeRate"></param>
    /// <param name="ConnectionString"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Saves a list of exchange rates to the database.
    /// </summary>
    /// <param name="exchanges"></param>
    /// <param name="ConnectionString"></param>
    /// <returns></returns>
    public static async Task<bool> SaveRatesAsync(List<Exchange> exchanges, string ConnectionString)
    {
        int cnt = 0;
        foreach (Exchange exchange in exchanges)
        {
            if ( await SaveRateAsync(exchange, ConnectionString))
                cnt++;
        }
        return (cnt == exchanges.Count);
    }
    /// <summary>
    /// Caches the data to a json file
    /// </summary>
    /// <param name="exchanges"></param>
    /// <param name="cacheFile"></param>
    /// <returns></returns>
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
