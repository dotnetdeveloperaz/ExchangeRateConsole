using System.ComponentModel;
using ExchangeRateConsole.Commands.Settings;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class StatusCommand : AsyncCommand<StatusCommand.Settings>
{
    public class Settings : BaseCommandSettings
    {
        [Description("Get Web API Status.")]
        [DefaultValue(false)]
        public bool GetStatus { get; set; }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.GetStatus = true;
        AnsiConsole.Write(
            new Markup(
                $"[red bold]Executed GetStatus[/] Execute? {settings.GetStatus} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            )
        );
        return Task.FromResult(0);
    }
}
