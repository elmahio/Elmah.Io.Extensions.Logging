using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Elmah.Io.Extensions.Logging.AspNetCore.Net10.Pages
{
    public class IndexModel(ILogger<IndexModel> logger) : PageModel
    {
        public void OnGet()
        {
            logger.LogInformation("This is an information message"); // Not logged as default
            logger.LogWarning("This is a warning message");
        }
    }
}
