using Microsoft.AspNetCore.Mvc;

namespace EgitimPortaliUI.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }
    }
}