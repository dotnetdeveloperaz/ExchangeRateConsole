using Newtonsoft.Json;
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

    public static Exchange GetExchangeRate(string Uri, bool Save)
    {
        var client = new HttpClient();
        var response = client.GetAsync(Uri).GetAwaiter().GetResult();
        var info = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        var exchangeRate = JsonConvert.DeserializeObject<Exchange>(info);

        // Need to add save functionality.

        return exchangeRate;
    }

    public static List<Exchange> GetExchangeRates(string Uri, string StartDate, string EndDate, bool Save)
    {
        DateTime startDate = DateTime.Parse(StartDate);
        DateTime endDate = DateTime.Parse(EndDate);

        exchangeRates = new List<Exchange>();
        while (startDate <= endDate)
        {
            var url = Uri.Replace("{date}", startDate.ToString("yyyy-MM-dd"));
            exchangeRates.Add(GetExchangeRate(url, Save));
            startDate = startDate.AddDays(1);
        }

        return exchangeRates;
    }
}
