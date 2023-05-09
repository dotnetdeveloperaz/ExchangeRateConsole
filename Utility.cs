using System.Data;
using System.Reflection;
using Newtonsoft.Json;
using MySqlConnector;
using PublicHoliday;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole;

public class Utility
{
    private static List<DateTime> listOfDays;
    private static List<Exchange> exchangeRates;

    public List<DateTime> ListOfDays
    {
        get { return listOfDays; }
    }

    public List<Exchange> ExchangeRates
    {
        get { return exchangeRates; }
    }

    public static int GetNumberOfDays(DateTime start, DateTime end)
    {
        listOfDays = new List<DateTime>();
        int i = 0;
        var res = DateTime.Compare(end, DateTime.Now);

        while (start <= end)
        {
            bool isHoliday = new USAPublicHoliday().IsPublicHoliday(start);
            if (
                !isHoliday
                && start.DayOfWeek != DayOfWeek.Saturday
                && start.DayOfWeek != DayOfWeek.Sunday
            )

                i++;
            listOfDays.Add(start);
            start = start.AddDays(1);
        }
        return i;
    }

    public static bool IsHolidayOrWeekend(string Date)
    {
        DateTime date = DateTime.Parse(Date);
        bool isHoliday = new USAPublicHoliday().IsPublicHoliday(date);
        if (isHoliday || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;
        else
            return false;
    }

    public static Exchange GetExchangeRate(string Uri, bool Save, string ConnectionString)
    {
        var client = new HttpClient();
        var response = client.GetAsync(Uri).GetAwaiter().GetResult();
        var info = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var exchangeRate = JsonConvert.DeserializeObject<Exchange>(info);

        // Need to add save functionality.
        if(Save)
            SaveRate(exchangeRate, ConnectionString);

        return exchangeRate;
    }

    public static List<Exchange> GetExchangeRates(
        string Uri,
        string StartDate,
        string EndDate,
        bool Save,
        string ConnectionString
    )
    {
        DateTime startDate = DateTime.Parse(StartDate);
        DateTime endDate = DateTime.Parse(EndDate);

        exchangeRates = new List<Exchange>();
        while (startDate <= endDate)
        {
            var url = Uri.Replace("{date}", startDate.ToString("yyyy-MM-dd"));
            if(!IsHolidayOrWeekend(startDate.ToString("yyyy-MM-dd")))
                exchangeRates.Add(GetExchangeRate(url, Save, ConnectionString));
            startDate = startDate.AddDays(1);
        }

        return exchangeRates;
    }

    public static void SaveRate(Exchange ExchangeRate, string ConnectionString)
    {
        var rates = ExchangeRate.rates;

        foreach (PropertyInfo prop in rates.GetType().GetProperties())
        {
            if (prop.GetValue(rates).ToString() != "0")
            {
                var Symbol = prop.Name;
                var Rate = double.Parse(prop.GetValue(rates).ToString());
                var RateDate = ExchangeRate.RateDate.ToString("yyyy-MM-dd");

                MySqlConnection sqlConnection = new MySqlConnection(ConnectionString);
                MySqlCommand sqlCommand = new MySqlCommand("usp_AddExchangeRate", sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;
                try
                {
                    sqlConnection.Open();
                    sqlCommand.Parameters.AddWithValue("symbol", Symbol);
                    sqlCommand.Parameters.AddWithValue("rate", Rate);
                    sqlCommand.Parameters.AddWithValue("ratedate", RateDate);
                    var recs = sqlCommand.ExecuteNonQuery();
                }
                catch (MySqlException)
                {
                    //Console.WriteLine($"Exception: {ex.Message}");
                    /// TODO
                    // We need to add error bubble up to the caller
                    // in the case this is the date range call so
                    // that we can save the results for later saving
                    // to avoid doubling up on the webapi call to the
                    // third party.
                    string result = JsonConvert.SerializeObject(ExchangeRate, Formatting.Indented);
                    File.AppendAllText($"ExchangeRate.cache", result);
                    return;
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
    }
}
