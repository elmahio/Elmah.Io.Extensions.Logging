using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging.Function.Isolated
{
    public class Function1
    {
        private readonly ILogger _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("Function1")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request."); // <-- Don't go into elmah.io because of the filter set in Program.cs
            _logger.LogWarning("This is a warning");
            throw new Exception("This is an error");
        }
    }
}
