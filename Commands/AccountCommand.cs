using System.ComponentModel;
using System.Text.Json;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Reflection;
using ExchangeRateConsole.Models;
using ExchangeRateConsole.Commands.Settings;

namespace ExchangeRateConsole.Commands;

public class AccountCommand : AsyncCommand<AccountCommand.Settings>
{
    private readonly ApiServer _apiServer;
    private readonly string _connectionString;
    private ExchangeRateEventSource _eventSource;

    public AccountCommand(ApiServer config, ConnectionStrings ConnectionString, ExchangeRateEventSource eventSource)
    {
        _apiServer = config;
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
        _apiServer.AppId = settings.OverrideAppId ?? _apiServer.AppId;
        string url = _apiServer.BaseUrl + _apiServer.Usage + "?app_id=" + _apiServer.AppId;
        if (settings.Debug)
        {
            if (!DebugDisplay.Print(settings, _apiServer, _connectionString, url))
                return 0;
        }
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
        Account account = new();
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
                string msg = settings.IsFake ? "Loading Sample Data" : "Calling API";
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[red bold] {msg} To Get Account Details...[/]"
                        )
                );
                // Need to add check for IsFake and handle
                if (!settings.IsFake)
                    account = await GetAccountAsync(url);
                else
                {
                    string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string file = Path.Combine(path, "Account.sample");
                    if (File.Exists(file))
                    {
                        var json = File.ReadAllText(file);
                        account = JsonSerializer.Deserialize<Account>(json);
                    }
                }
                var data = account.Data;
                var plan = data.Plan;
                var usage = data.Usage;
                var features = plan.Features;
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[green bold]Retrieved Account Details...[/]"
                        )
                );
                Update(70, () => titleTable.AddRow($"[yellow bold]    Plan: {plan.Name}[/]"));
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Specify Base Symbols: {features.@Base}[/]"
                        )
                );
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Symbols: {features.Symbols}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow($"[yellow bold]    TimeSeries: {features.TimeSeries}[/]")
                );
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Convert: {features.Convert}[/]")
                );
                Update(70, () => titleTable.AddRow($"[yellow bold]    Quota: {plan.Quota}[/]"));
                Update(
                    70,
                    () => titleTable.AddRow($"[yellow bold]    Requests Made: {usage.Requests}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Remaining: {usage.RequestsRemaining}[/]"
                        )
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow($"[yellow bold]    Days Elapsed: {usage.DaysElapsed}[/]")
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Days Remaining: {usage.DaysRemaining}[/]"
                        )
                );
                Update(
                    70,
                    () =>
                        titleTable.AddRow(
                            $"[yellow bold]    Daily Average: {usage.DailyAverage}[/]"
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
