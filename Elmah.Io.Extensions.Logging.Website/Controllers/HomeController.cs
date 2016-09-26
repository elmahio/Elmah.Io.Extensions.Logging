using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Elmah.Io.Extensions.Logging.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILoggerFactory factory)
        {
            _logger = factory.CreateLogger("elmah.io");
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Request to index");

            return View();
        }

        public IActionResult About()
        {
            _logger.LogWarning("Request to about");

            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            _logger.LogError(1, new Exception(), "Request to contact");

            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
