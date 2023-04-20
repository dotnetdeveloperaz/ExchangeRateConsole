<h3 align="center">Exchange Rate Console</h3>

<p>Console to retrieve currency exchange rates. Free accounts do not let you specify the base currency, it is USD. This application does support the ability to pass in any base currency (if you sign up for a plan that supports it) and rate currency symbols. If no symbols are specified, it will return all currency rates against the USD (or the specified base current).
    <br> 
</p>

## Status

.NET 6
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet6.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet6.yml)

.NET 7
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet7.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet7.yml)

.NET 8 Preview 3
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet8.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet8.yml)

## Buy Me A Coffee
<a href="https://www.buymeacoffee.com/dotnetdev" target="_blank"><img src="https://cdn.buymeacoffee.com/buttons/default-orange.png" alt="Buy Me A Coffee" height="41" width="174"></a>

## Table of Contents
- [About](#about)
- [Getting Started](#getting_started)
- [Usage](#usage)
- [Built Using](#built_using)

## About <a name = "about"></a>

Simple console application for retrieving different (or all if --symbol is not used) currency exchange rates from a third party API <a target="_blank" href="https://openexchangerates.org/api/">Open Exchange Rates</a> and optionally saving the data to a MySql/MariaDB database.

## Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine.

### Prerequisites

- .NET 6, 7, or 8 preview (1 thru 3)
- Account with <a target="_blank" href="https://openexchangerates.org/api/">Open Exchange Rates</a> (Free accounts with 1,000 calls per month available)
- MariaDB (or MySql)

### Installing

*Optional* Restore the database in the db folder (it only contains historical data for EUR and TRY)
Extract ExchangeRates.sql from the ExchangeRates.sql.gz archive located in the db folder.
```
mysql -u [user] -p [database name] < ExchangeRates.sql
```
Table creation, run ExchangeRates-table.sql
Stored procedure, run usp_AddExchangeRate.sql

Set your configuration for AppId for the Open Exchange API and database connection string. You can also add them to user secrets.
```
dotnet user-secrets init
dotnet user-secrets set "AppId" "Your AppId"
dotnet user-secrets set "ConnectionStrings:DefaultDB" "<Your database connection string>"
```

Build the project by running the following in the project folder.
``` 
dotnet build
``` 
To run a simple test, run the following, specifying the .NET version to run on (net6.0, net7.0, net8.0).
``` 
dotnet run --framework net6.0
```
*Note:* You can remove multi-framework targeting from ExchangeRateConsole.csproj to target a specific version and omit --framework.

To run a test after you create an account with Open Exchange
```
dotnet run acct --framework net6.0
```
You should see something similar to:
```
╭──────────────────────────────────────────────────────────────────────────────────────────────────╮
│                                  Retrieving Account Information                                  │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│ ⏳ Calling API To Get Account Details...                                                         │
│ ✔ Retrieved Account Details...                                                                  │
│     Plan: Free                                                                                   │
│     Specify Base Symbols: False                                                                  │
│     Symbols: False                                                                               │
│     TimeSeries: False                                                                            │
│     Convert: False                                                                               │
│     Quota: 1000 requests / month                                                                 │
│     Requests Made: 176                                                                           │
│     Remaining: 824                                                                               │
│     Days Elapsed: 14                                                                             │
│     Days Remaining: 17                                                                           │
│     Daily Average: 12                                                                            │
╰──────────────────────────────────────────────────────────────────────────────────────────────────╯
```
To test your database connection string, you can run the following:
```
dotnet run testdb --framework net6.0
```
You should see the following screen:
```
╭──────────────────────────────────────────────────────────────────────────────────────────────────╮
│                          Running Database Connection Configuration Test                          │
├──────────────────────────────────────────────────────────────────────────────────────────────────┤
│ ⏳ Testing Connection...                                                                         │
│ ✔ Connection Successful                                                                         │
╰──────────────────────────────────────────────────────────────────────────────────────────────────╯
```

### Usage <a name="usage"></a>
dotnet run [command] [switches]

The individual commands are:
```
range       Gets exchange rates between two date ranges

getrate     Gets the current rate or for the day specified

acct        Gets account details

status      Gets the WebApi Status (available or not)

testdb      Tests your database connection string
```
The available switches are:
```
--symbols   These are commma separated, no space.
            --switch EUR,TRY
--save      Saves the exchange rate data to the database
--symbols   The exchange rate(s) to get <TRY,EUR>
--base      The base currency to get exchange rates against. Free accounts only support USD.
*These three switchs are available on range and getrate*

--date      This is to specify the date for command get rate <YYYY-MM-DD>
--startdate This specifies the start date for command range <YYYY-MM-DD>
--enddate   This specifies the start date for command range <YYYY-MM-DD>
*Only Non-Holiday Week Days Are Processed*

--debug     Available on all commands
            shows configuration data
--hidden    Available on all commands
            Shows all secret configuration            
```

```
dotnet run
```
This gives you the following screen.

```
USAGE:
    ExchangeRateConsole.dll [OPTIONS] <COMMAND>

EXAMPLES:
    ExchangeRateConsole.dll range --start YYYY-MM-DD --end YYYY-MM-DD --symbols EUR,TRY --base USD 
--debug --hidden
    ExchangeRateConsole.dll getrate --symbols EUR,TRY --base USD --date YYYY-MM-DD --save --debug 
--hidden
    ExchangeRateConsole.dll acct --debug --hidden
    ExchangeRateConsole.dll status --debug --hidden
    ExchangeRateConsole.dll testdb --debug --hidden

OPTIONS:
    -h, --help       Prints help information   
    -v, --version    Prints version information

COMMANDS:
    restore    Restores cache from failed completion                                                
    account    Gets account information                                                             
    range      Gets historical Exchange rate(s). Use --save to save to the database.                
               Weekends and holidays are skipped because markets are closed                         
    getrate    Gets the current Exchange rate(s). Use --save to save to database. Weekends and      
               holidays are skipped                                                                 
    acct       Gets Account Statistics                                                              
    status     Gets WebApi Status                                                                   
    testdb     Tests The Database Connection   
```

### Built Using <a name = "built_using"></a>

- [MariaDB](https://www.mariadb.com/) - Database (MySQL should be compatible as well)
- [.NET 6, 7 and 8 preview](https://https://dotnet.microsoft.com/) - Application Framework
- [Exchange Rate Web Api](https://openexchangerates.org/)
- [Spectre Console and Spectra Cli](https://spectreconsole.net/)
