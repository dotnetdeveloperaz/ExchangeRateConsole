using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using ExchangeRateConsole.Models;

namespace ExchangeRateConsole.Commands;

public class AccountCommand : AsyncCommand<AccountCommand.Settings>
{
    private readonly ApiServer _config;
    private readonly string _connectionString;
    private ExchangeRateEventSource _eventSource;

    public AccountCommand(ApiServer config, ConnectionStrings ConnectionString, ExchangeRateEventSource eventSource)
    {
        _config = config;
        _connectionString = ConnectionString.DefaultDB;
        _eventSource = eventSource;
    }

    public class Settings : BaseCommandSettings
    {
        [Description("Get Account Statistics.")]
        [DefaultValue(false)]
        public bool GetAccount { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        settings.GetAccount = true;
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
        Account account;
        // Animate
        await AnsiConsole
            .Live(titleTable)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .StartAsync(async ctx =>
            {
                void Update(int delay, Action action)
                {
                    action();
                    ctx.Refresh();
                    Thread.Sleep(delay);
                }

                if (settings.Debug)
                {
                    if (settings.ShowHidden)
                    {
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]WebAPI AppId: [/][blue]{_config.AppId.Replace("?app_id=", "")}[/]"
                                )
                        );
                        Update(
                            70,
                            () =>
                                titleTable.AddRow(
                                    $"[red bold]Database: [/][blue]{_connectionString}[/]"
                                )
                        );
                    }
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Url: [/][blue]{_config.BaseUrl}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]History Url: [/][blue]{_config.History}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Latest Url: [/][blue]{_config.Latest}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Usage Url: [/][blue]{_config.Usage}[/]"
                            )
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Base Symbol: [/][blue]{_config.BaseSymbol}[/]"
                            )
                    );
                    Update(
                        70,
                        () => titleTable.AddRow($"[red bold]Debug: [/][blue]{settings.Debug}[/]")
                    );
                    Update(
                        70,
                        () =>
                            titleTable.AddRow(
                                $"[red bold]Show Secret: [/][blue]{settings.ShowHidden}[/]"
                            )
                    );
                }
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[red bold] Calling API To Get Account Details...[/]"
                        )
                );
                /*              
                                var client = new HttpClient();
                                var response = await client.GetAsync(_config.BaseUrl + _config.Usage + "?app_id=" + _config.AppId);
                                var info = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                                account = JsonConvert.DeserializeObject<Account>(info);
                */
                account = await GetAccountAsync(_config.BaseUrl + _config.Usage + "?app_id=" + _config.AppId);
                var data = account.data;
                var plan = data.plan;
                var usage = data.usage;
                var features = plan.features;
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[green bold] Retrieved Account Details...[/]"
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
            });
        return 0;
    }
    private static async Task<Account> GetAccountAsync(string url)
    {
        Account account;
        using (HttpClient client = new HttpClient())
        {
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                HttpResponseMessage response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStreamAsync();
                account = await JsonSerializer.DeserializeAsync<Account>(result);
            }
        }
        return account;
    }

}
