﻿using System.ComponentModel;
using Spectre.Console.Cli;

namespace ExchangeRateConsole.Commands.Settings
{
    public class BaseCommandSettings : CommandSettings
    {
        [CommandOption("--debug")]
        [Description("Enable Debug Output")]
        [DefaultValue(false)]
        public bool Debug { get; set; }

        [CommandOption("--hidden")]
        [Description("Enable User Secret Debug Output")]
        [DefaultValue(false)]
        public bool ShowHidden { get; set; }

        [CommandOption("--fake")]
        [Description("Does Not Call WebApi")]
        [DefaultValue(false)]
        public bool IsFake { get; set; }

        [CommandOption("--appid")]
        [Description("Overrides The AppId")]
        [DefaultValue(null)]
        public string OverrideAppId { get; set; } 

        [CommandOption("--cachefile")]
        [Description("Cache File to Use - Override Default")]
        [DefaultValue(null)]
        public string CacheFile { get; set; } = null;  
    }
}
