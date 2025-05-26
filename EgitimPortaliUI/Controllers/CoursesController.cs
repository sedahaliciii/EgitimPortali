using Microsoft.AspNetCore.Mvc;

namespace EgitimPortaliUI.Controllers
{
    public class CoursesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.CourseId = id;
            return View();
        }
    }
}