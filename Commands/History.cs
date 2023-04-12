using System;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands;

public class HistoryCommand : Command<HistoryCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [Description("Get Rate For Specified Date")]
        [DefaultValue(false)]
        public bool GetRate { get; set; }

        [CommandOption("--date <YYYY-MM-DD>")]
        [Description("Date To Get Rate(s) For")]
        public string Date { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        [DefaultValue("USD")]
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
        settings.GetRate = true;
        if(settings.Date == null)
            settings.Date = DateTime.Now.ToString("yyyy-MM-dd");
            AnsiConsole.Write(new Markup(
            $"[red bold]Executed History[/] Execute? {settings.GetRate} Date: {settings.Date} Base: {settings.BaseSymbol} Save: {settings.Save} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            ));
    return 0;
    }
}
