using Microsoft.AspNetCore.Mvc;

namespace EgitimPortaliUI.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // JavaScript ile oturum kontrolü yapmak için ViewBag'e flag ekle
            ViewBag.RequireAuth = true;
            return View();
        }
    }
}