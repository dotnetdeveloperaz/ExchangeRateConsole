namespace ExchangeRateConsole.Models;

public class ExchangeRate
{
    public string Symbol { get; set; }
    public double Rate { get; set; }
    public DateTime RateDate { get; set; }
}