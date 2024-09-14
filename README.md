<h3 align="center">Exchange Rate Console</h3>

<p>Console to retrieve currency exchange rates. Free accounts do not let you specify the base currency, it is USD. This application does support the ability to pass in any base currency (if you sign up for a plan that supports it) and rate currency symbols. If no symbols are specified, it will return all currency rates against the USD (or the specified base currency).
</p>

## Status

.NET 8 
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

- .NET 8.
- Account with <a target="_blank" href="https://openexchangerates.org/api/">Open Exchange Rates</a> (Free accounts with 1,000 calls per month available)
- MariaDB or MySql (optional as you can use this application without a database).

### Installing

*Optional* Restore the database in the db folder (it only contains historical data for EUR and TRY)
Extract ExchangeRates.sql from the ExchangeRates.sql.gz archive located in the db folder.
```
gzip -d ExchangeRates.sql.gz
```
Run the query using the command below or in your MySql/MariaDB Database Query Tool of choice. This will include historical data for EUR and TRY.
```
mysql -u [user] -p [database name] < ExchangeRates.sql
```
You can also just create the database table and stored procedures for a fresh install with no data.
```
mysql -u [user] -p [database name] < ExchangeRates-table.sql
mysql -u [user] -p [database name] < usp_AddExchangeRate.sql
mysql -u [user] -p [database name] < usp_GetExchangeRates.sql
```

Set your configuration for AppId for the Open Exchange API and database connection string. You can also add them to user secrets running the following:
```
dotnet user-secrets init
dotnet user-secrets set "ApiServer:AppId" "<Your AppId>"
dotnet user-secrets set "ConnectionStrings:DefaultDB" "<Your database connection string>"
```

Build the project by running the following in the project folder.
``` 
dotnet build
``` 
To run a simple test, run the following, specifying the .NET version to run on (net6.0, net7.0, net8.0). If you will target a single .NET framework, just change the project file for a single framework version.
``` 
dotnet run
```
To run a test after you create an account with Open Exchange
```
dotnet run acct
```
You should see something similar to:
```
╭─────────────────────────────────────────────────────────────────────────╮
│                       Exchange Rate Console v3.0                        │
│                        Written By Scott Glasgow                         │
╰─────────────────────────────────────────────────────────────────────────╯
╭─────────────────────────────────────────────────────────────────────────╮
│                     Retrieving Account Information                      │
├─────────────────────────────────────────────────────────────────────────┤
│  Calling API To Get Account Details...                                  │
│ Retrieved Account Details...                                            │
│     Plan: Free                                                          │
│     Specify Base Symbols: False                                         │
│     Symbols: False                                                      │
│     TimeSeries: False                                                   │
│     Convert: False                                                      │
│     Quota: 1000 requests / month                                        │
│     Requests Made: 84                                                   │
│     Remaining: 916                                                      │
│     Days Elapsed: 14                                                    │
│     Days Remaining: 17                                                  │
│     Daily Average: 6                                                    │
╰─────────────────────────────────────────────────────────────────────────╯
```
To test your database connection string, you can run the following:
```
dotnet run testdb
```
If you would like to test a connectionstring that differs from what is configured run the command with the --db option with your connection string in quotes.
```
dotnet run testdb --db "<YourConnectionString>"
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

view        Views exchange rates from either the cachefile or database using parameters
            passed.

cachestats  Displays the statistics for the cache file if one exists.

acct        Gets your webapi account details from OpenExchangeRates

status      Gets the WebApi Status (available or not)

testdb      Tests your database connection string
```
The available switches are:
```
--save      Saves the exchange rate data to the database or cachefile when --cache is used
--cache     Specifies to use cache
--cachefile Overrides the default (configured) cache file to use.
--symbols   The exchange rate(s) to get <TRY,EUR>
--base      The base currency to get exchange rates against. Free accounts only support USD.
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
╭──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╮
│                                                Exchange Rate Console v3.0                                                │
│                                                 Written By Scott Glasgow                                                 │
╰──────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────╯
USAGE:
    ExchangeRateConsole.dll [OPTIONS] <COMMAND>

EXAMPLES:
    ExchangeRateConsole.dll rate --start YYYY-MM-DD --end YYYY-MM-DD --symbols EUR,TRY --base USD --appid <AppID> --debug 
--hidden --save --cache --fake --appid <AppId>
    ExchangeRateConsole.dll rate --symbols EUR,TRY --base USD --date YYYY-MM-DD --save --appid AppID --debug --hidden
    ExchangeRateConsole.dll view --start YYYY-MM-DD --end YYYY-MM-DD --symbols EUR,TRY --base USD --fake --cache --cachefile
<file> --debug --appid <AppID>
    ExchangeRateConsole.dll cachestats --cachefile <filename>
    ExchangeRateConsole.dll testdb --db '<YourDBConnectionString>' --debug --hidden

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    range         Gets historical Exchange rate(s). Use --save to save to the database.                                     
                  Weekends and holidays are skipped because markets are closed                                              
    rate          Gets the current Exchange rate(s). Use --save to save to database. Weekends and holidays are skipped      
    view          Works like the Range command except it displays data from the configured database or from the configured  
                  cachefile with --cache. Use with --cachefile </path/filename to override.                                 
                                                                                                                            
    cachestats    Displays the cachefile statistics, start and end dates.                                                   
                  To override configured cache file, use the --cachefile </path/filename> switch.                           
                                                                                                                            
    testdb        Tests the configured database connection.                                                                 
                  Use the --db "<YourConnectionString>" (Quotes Required!) to test connectionstrings for diagnosing.        
                  This switch is NOT available with any other command.                                                      
                                                                                                                            
    restore       Writes data from cache file to database and deletes the cache file after successful completion            
    account       Gets account information                                                                                  
    status        Gets WebApi Status   
```

### Built Using <a name = "built_using"></a>

- [MariaDB](https://www.mariadb.com/) OR [MySQL](https://www.mysql.com/)
- [.NET 8](https://https://dotnet.microsoft.com/) - Application Framework
- [Exchange Rate Web Api](https://openexchangerates.org/)
- [Spectre Console and Spectra Cli](https://spectreconsole.net/)
- [Public Holiday](https://github.com/martinjw/Holiday)
