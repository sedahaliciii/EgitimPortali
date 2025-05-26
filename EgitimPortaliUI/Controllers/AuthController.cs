using Microsoft.AspNetCore.Mvc;

namespace EgitimPortaliUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Login()
        {
            // API URL'ini view'a gönder
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult Register()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult ForgotPassword()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult Logout()
        {
            TempData["ClearLocalStorage"] = true;
            return RedirectToAction("Login");
        }
    }
}