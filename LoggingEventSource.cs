namespace ExchangeRateConsole;

[EventSource(Name = "Exchange Rate Console")]
public class ExchangeRateEventSource : EventSource
{
    public static ExchangeRateEventSource Log = new ExchangeRateEventSource();

    public void Trace(string message)
    {
        if (IsEnabled())
            WriteEvent(1, message);
    }
}