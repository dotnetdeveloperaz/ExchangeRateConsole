using System;
using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using Newtonsoft.Json;

namespace ExchangeRateConsole.Commands;

public class GetRateCommand : Command<HGetRateCommand.Settings>
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
        if (settings.Date == null)
            settings.Date = DateTime.Now.ToString("yyyy-MM-dd");
        AnsiConsole.Write(
            new Markup(
                $"[red bold]Executed History[/] Execute? {settings.GetRate} Date: {settings.Date} Base: {settings.BaseSymbol} Save: {settings.Save} Debug: {settings.Debug} Hidden: {settings.ShowHidden}"
            )
        );
        var titleTable = new Table().Centered();
        // Borders
        titleTable.BorderColor(Color.Blue);
        titleTable.MinimalBorder();
        titleTable.SimpleBorder();
        titleTable.AddColumn(
            new TableColumn(new Markup("[yellow bold]Retrieving Account Information[/]").Centered())
        );
        titleTable.BorderColor(Color.Blue);
        titleTable.Border(TableBorder.Rounded);
        titleTable.Expand();
//        Exchange exchange;
        // Animate
        AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $":hourglass_not_done:[red bold] Calling API To Get Account Details...[/]"
                        )
                );
/*                var client = new HttpClient();
                var response = client
                    .GetAsync(
                        Configure.Configuration.BaseURL
                            + Configure.Configuration.History
                            + Configure.Configuration.AppId
                    )
                    .GetAwaiter()
                    .GetResult();
                var info = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                if (settings.DisplayJson)
                {
                    Update(70, () => titleTable.AddRow($"[red]Data: {info}[/]"));
                }
*/                if (settings.Debug)
                {
                    if (settings.ShowHidden)
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Calling WebAPI URL: [/][blue]{Configure.Configuration.BaseURL}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]WebAPI AppId: [/][blue]{Configure.Configuration.AppId.Replace("?app_id=", "")}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Database Connection: [/][blue]{Configure.Configuration.DefaultDB}[/]"
                                )
                        );
                    }
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Url: [/][blue]{Configure.Configuration.BaseURL}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]History Url: [/][blue]{Configure.Configuration.History}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Latest Url: [/][blue]{Configure.Configuration.Latest}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Usage Url: [/][blue]{Configure.Configuration.Usage}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Symbol: [/][blue]{Configure.Configuration.BaseSymbol}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Debug: [/][blue]{settings.Debug}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Date To Get Rate(s): [/][blue]{settings.Date}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Save: [/][blue]{settings.Save}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Secret: [/][blue]{settings.ShowHidden}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Fake: [/][blue]{settings.IsFake}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Json: [/][blue]{settings.DisplayJson}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Pretty: [/][blue]{settings.Pretty}[/]"
                            )
                    );
                }
/*               account = JsonConvert.DeserializeObject<Account>(info);
                var data = account.data;
                var plan = data.plan;
                var usage = data.usage;
                var features = plan.features;
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $":check_mark:[green bold] Retrieved Account Details...[/]"
                        )
                );
                Update(70, () => titleTable.AddRow($"[yellow bold]    Plan: {plan.name}[/]"));
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Specify Base Symbols: {features.@base}[/]"
                        )
                );
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Symbols: {features.symbols}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow($"[yellow bold]    TimeSeries: {features.TimeSeries}[/]")
                );
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Convert: {features.convert}[/]")
                );
                Update(70, () => titleTable.AddRow($"[yellow bold]    Quota: {plan.quota}[/]"));
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Requests Made: {usage.requests}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Remaining: {usage.requests_remaining}[/]"
                        )
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow($"[yellow bold]    Days Elapsed: {usage.days_elapsed}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Days Remaining: {usage.days_remaining}[/]"
                        )
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Daily Average: {usage.daily_average}[/]"
                        )
                );
*/            });
        return 0;
    }
}
