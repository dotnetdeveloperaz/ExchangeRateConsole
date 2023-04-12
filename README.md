<h3 align="center">Exchange Rate Console</h3>

<p>Console to retrieve currency exchange rates. Currently only support base USD and TRY/EUR rates are the defaults. Has built-in support to pass in any base currency and rate currency symbols.
    <br> 
</p>

## Status

.NET 6
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet6.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet6.yml)

.NET 7
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet7.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet7.yml)

.NET 8 Preview
[![build](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet8.yml/badge.svg?branch=main)](https://github.com/dotnetdeveloperaz/ExchangeRateConsolev2/actions/workflows/dotnet8.yml)

## Table of Contents
- [About](#about)
- [Getting Started](#getting_started)
- [Deployment](#deployment)
- [Usage](#usage)
- [Built Using](#built_using)

## About <a name = "about"></a>

Simple console application for retrieving different currency exchange rates from a third party API <a target="_blank" href="https://openexchangerates.org/api/">Open Exchange Rates</a> and optionally saving the data to a database.

## Getting Started <a name = "getting_started"></a>

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See [deployment](#deployment) for notes on how to deploy the project on a live system.

### Prerequisites

- .NET 6, 7, or 8 preview
- MySqlConnector
- Account with <a target="_blank" href="https://openexchangerates.org/api/">Open Exchange Rates</a> (Free accounts with 1,000 calls per month available)
- MariaDB (or MySql) and Exchange Rate table for storing exchange rates.

### Installing

Create the tables and stored procedures used by this utility.
> *Need to add MariaDB/MySql database restore (or clean install) steps.*

Build the project by running the following in the project folder.
``` 
dotnet build
``` 
* This will change after refactoring
To run a simple test, run the following.
``` 
dotnet run /account
```
You should see something similar to:
```
Calling https://openexchangerates.org/api/usage.json
Plan Subscription: Free
        Specify Base: False
        Symbols: False
        Experimental: True

Quota: 1000
Request Left: 971
Days Remaining: 22
Daily Average: 3
```

### Usage <a name="usage"></a>

Run dotnet run which will use default USD base and retrieve TRY and EUR currency rates.
Parameters that can be passed: 

/base {xxx} - *USD is the default and free accounts cannot specify the base currency. 
```
/base EUR
```

/symbols {xxx,xxx} - One or more currency symbols to get the exchange rate for.
```
/symbols EUR,TRY
```
Note: xxx is the currency symbol. For the /symbols parameter, they are comma separated, no spaces.

/fake - This will run without making an actual call to the web service and use default values for testing.

### Deployment <a name = "deployment"></a>

Add additional notes about how to deploy this on a live system.

### Built Using <a name = "built_using"></a>

- [MariaDB](https://www.mariadb.com/) - Database (MySQL should be compatible as well)
- [.NET 6](https://https://dotnet.microsoft.com/) - Application Framework
- [Exchange Rate Web Api](https://openexchangerates.org/)
