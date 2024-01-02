using Spectre.Console;
using System.Reflection;

namespace ExchangeRateConsole;

class Title
{
    public static void Print()
    {
        AssemblyName assembly = typeof(Title).Assembly.GetName();
        var version = $"v{assembly.Version.Major}.{assembly.Version.Minor}";
        Console.Clear();
        var titleTable = new Table().Centered();
        titleTable.AddColumn(
            new TableColumn(
                new Markup(
                    $"[blue bold]Exchange Rate Console[/][yellow bold italic] {version}[/]\r\n[green bold italic]Written By Scott Glasgow[/]"
                )
            ).Centered()
        );
        titleTable.BorderColor(Color.Blue);
        titleTable.Border(TableBorder.Rounded);
        titleTable.Expand();

        AnsiConsole.Write(titleTable);
    }
}
