using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateConsole.Commands.Settings
{
    public class RateCommandSettings : BaseCommandSettings
    {
        [CommandOption("--date <YYYY-MM-DD>")]
        [Description("Get Rate For Specified Date")]
        [DefaultValue(null)]
        public string Date { get; set;}

        [CommandOption("--start <YYYY-MM-DD>")]
        [Description("Start Date")]
        [DefaultValue("")]
        public string StartDate { get; set; }

        [CommandOption("--end <YYYY-MM-DD>")]
        [Description("End Date (optional)")]
        [DefaultValue("")]
        public string EndDate { get; set; }

        [CommandOption("--base <Symbol>")]
        [Description("Base Symbol To Use To Convert From")]
        [DefaultValue("USD")]
        public string BaseSymbol { get; set; }

        [CommandOption("--symbols <EUR>")]
        [Description("Exchange Rate(s) To Get")]
        public string Symbols { get; set; }

        [CommandOption("--cache")]
        [Description("Cache Results To File")]
        [DefaultValue(false)]
        public bool Cache { get; set; }

        [CommandOption("--cachefile")]
        [Description("Cache File to Use - Override Default")]
        [DefaultValue(null)]
        public string CacheFile { get; set; } = null;        
        
        [CommandOption("--save")]
        [Description("Save Results")]
        [DefaultValue(false)]
        public bool Save { get; set; }
    }
}
