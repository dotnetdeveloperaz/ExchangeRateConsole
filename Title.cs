using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole;

class Title
{
    public static void Print()
    {
        Console.Clear();
        var titleTable = new Table().Centered();
        titleTable.AddColumn(
            new TableColumn(
                new Markup(
                    $"[blue bold]Exchange :chart_increasing: Rate Console[/] v{Configure.Version}\r\n[green bold italic]Copyright © 2023 Scott Glasgow[/]"
                )
            ).Centered()
        );
        titleTable.BorderColor(Color.Blue);
        titleTable.Border(TableBorder.Rounded);
        titleTable.Expand();

        AnsiConsole.Write(titleTable);
    }
}
