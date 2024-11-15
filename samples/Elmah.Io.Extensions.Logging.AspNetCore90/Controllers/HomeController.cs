using Elmah.Io.Extensions.Logging.AspNetCore90.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Elmah.Io.Extensions.Logging.AspNetCore90.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("This is an information message"); // Not logged as default
            _logger.LogWarning("This is a warning message");
            return View();
        }

        public IActionResult Privacy()
        {
            try
            {
                var i = 0;
                var result = 42 / i;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error during Privacy");
            }

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
