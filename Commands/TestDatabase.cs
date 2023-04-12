using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class TestDatabaseCommand : Command<TestDatabaseCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Web API Status.")]
        [DefaultValue(false)]
        public bool DoTestDatabase { get; set; }

        [CommandOption("--debug")]
        [Description("Enable Debug Output")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [CommandOption("--hidden")]
        [Description("Enable Secret Debug Output")]
        [DefaultValue(false)]
        public bool ShowHidden { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        settings.DoTestDatabase = true;
        AnsiConsole.Write(new Markup(
            $"[red bold]Executed TestDatabase[/] Execute? {settings.DoTestDatabase} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            ));
        return 0;
    }
}
