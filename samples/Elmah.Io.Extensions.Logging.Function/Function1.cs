using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging.Function
{
    public static class Function1
    {
        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}"); // <-- Don't go into elmah.io because of the filter set in Startup.cs
            log.LogWarning("This is a warning");
            throw new Exception("This is an error");
        }
    }
}
