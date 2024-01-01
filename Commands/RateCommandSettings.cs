using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateConsole.Commands
{
    public class RateCommandSettings : BaseCommandSettings
    {
        [CommandOption("--start <YYYY-MM-DD>")]
        [Description("Start Date")]
        public string StartDate { get; set; }

        [CommandOption("--end <YYYY-MM-DD>")]
        [Description("End Date (optional)")]
        public string EndDate { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        [DefaultValue("USD")]
        public string BaseSymbol { get; set; }

        [CommandOption("--symbols <EUR>")]
        [Description("Exchange Rate(s) To Get")]
        public string Symbols { get; set; }

        [CommandOption("--save")]
        [Description("Save Results")]
        [DefaultValue(false)]
        public bool Save { get; set; }
    }
}
