#pragma warning disable S125 // Sections of code should not be commented out
#pragma warning disable CS8604 // Possible null reference argument.
using Elmah.Io.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

var config = builder.Configuration;
var apiKey = config["apiKey"];
var logId = new Guid(config["logId"]);

builder.Logging.AddElmahIo(o =>
{
    o.ApiKey = apiKey;
    o.LogId = logId;

    // Optional application name
    //o.Application = "Isolated Functions Application";

    // Additional options can be configured like this:
    //o.OnMessage = msg =>
    //{
    //    msg.Version = "10.0.0";
    //};

    // Example: Filter out all logged messages containing "IgnoreMe"
    //o.OnFilter = msg =>
    //{
    //    return msg.Title != null && msg.Title.Contains("IgnoreMe");
    //};
}
);
builder.Logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Warning);

var host = builder.Build();
await host.RunAsync();
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore S125 // Sections of code should not be commented out