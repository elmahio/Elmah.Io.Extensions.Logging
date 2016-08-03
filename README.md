# Elmah.Io.Extensions.Logging

[![Build status](https://ci.appveyor.com/api/projects/status/eiw9tpstm67t02v6?svg=true)](https://ci.appveyor.com/project/ThomasArdal/elmah-io-extensions-logging)
[![NuGet](https://img.shields.io/nuget/vpre/Elmah.Io.Extensions.Logging.svg)](https://www.nuget.org/packages/Elmah.Io.Extensions.Logging)

Log to [elmah.io](https://elmah.io/) from [Microsoft.Extensions.Logging](https://github.com/aspnet/Logging).

## Installation
Elmah.Io.Extensions.Logging installs through NuGet:

```
PS> Install-Package Elmah.Io.Extensions.Logging
```

Configure the elmah.io provider through code:

```c#
var factory = new LoggerFactory();
factory.AddElmahIo("API_KEY", new Guid("LOG_ID"));
var logger = factory.CreateLogger("MyLog");
```

In the example we create a new `LoggerFactory` and add the elmah.io provider using the `AddElmahIo` extension method, an API key and the Guid of the desired log. Finally, we create a new logger which is used later in this example to log messages to elmah.io.

## Usage
Log messages to elmah.io, just as with every other provider:

```c#
logger.LogInformation("This is information");
```
