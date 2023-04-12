using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class AccountCommand : Command<AccountCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Account Statistics.")]
        [DefaultValue(false)]
        public bool GetAccount { get; set; }

        [CommandOption("--fake")]
        [Description("Displays Fake Data Instead Of Calling WebAPI")]
        [DefaultValue(false)]
        public bool Save { get; set; }

        [CommandOption("--json")]
        [Description("Display The Raw JSON Response")]
        [DefaultValue(false)]
        public bool DisplayJson { get; set; }

        [CommandOption("--pretty")]
        [Description("Display The Raw JSON In Friendly Format Instead Of Minified")]
        [DefaultValue(false)]
        public bool Pretty { get; set; }

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
        settings.GetAccount = true;
            AnsiConsole.Write(new Markup(
            $"[red bold]Executed Account[/] Execute? {settings.GetAccount} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            ));
        return 0;
    }
}
