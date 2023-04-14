using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class StatusCommand : Command<StatusCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Web API Status.")]
        [DefaultValue(false)]
        public bool GetStatus { get; set; }

        [CommandOption("--fake")]
        [Description("Displays Fake Data Instead Of Calling WebAPI")]
        [DefaultValue(false)]
        public bool Save { get; set; }

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
        settings.GetStatus = true;
        AnsiConsole.Write(
            new Markup(
                $"[red bold]Executed GetStatus[/] Execute? {settings.GetStatus} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            )
        );
        return 0;
    }
}
