using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EgitimPortaliUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;

        public AccountController(ILogger<AccountController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
            return View();
        }

        public IActionResult Register()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
            return View();
        }

        public IActionResult Profile()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiBaseUrl"];
            return View();
        }

        public IActionResult Logout()
        {
            // Client tarafı logout
            return RedirectToAction("Login");
        }
    }
}