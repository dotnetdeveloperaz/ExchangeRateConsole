using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class RangeCommand : Command<RangeCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Rates For A Date Range.")]
        [DefaultValue(false)]
        public bool GetRange { get; set; }

        [CommandOption("--start <startdate>")]
        [Description("Start Date.")]
        public string StartDate { get; set; }

        [CommandOption("--end <enddate>")]
        [Description("End Date")]
        public string EndDate { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        public string BaseSymbol { get; set; }

        [CommandOption("--save")]
        [Description("Save Results")]
        [DefaultValue(false)]
        public bool Save { get; set; }

        [CommandOption("--fake")]
        [Description("Displays Fake Data Instead Of Calling WebAPI")]
        [DefaultValue(false)]
        public bool IsFake { get; set; }

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
        settings.GetRange = true;
        AnsiConsole.Write(
            new Markup(
                $"[red bold]Executed Range[/] Execute? {settings.GetRange} StartDate: {settings.StartDate} EndDate: {settings.EndDate} Base: {settings.BaseSymbol} Save: {settings.Save} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            )
        );
        return 0;
    }
}
