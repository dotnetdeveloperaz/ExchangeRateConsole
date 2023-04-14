using PublicHoliday;

namespace ExchangeRateConsole;

public class Utility
{
    private static List<DateTime> listOfDays;
    public List<DateTime> ListOfDays { get { return listOfDays; } }

    public static int GetNumberOfDays(DateTime start, DateTime end)
    {
        listOfDays = new List<DateTime>();
        int i = 0;
        var res = DateTime.Compare(end, DateTime.Now);

        // We do not want the end date to be the current date or future date.
/*
        if (res >= 0)
        {
            end = DateTime.Now.AddDays(-1);
            endDate = end;
        }
*/
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
}