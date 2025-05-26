using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EgitimPortaliUI.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IConfiguration _configuration;

        public CategoriesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult Create()
        {
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult Edit(int id)
        {
            ViewBag.CategoryId = id;
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }

        public IActionResult Details(int id)
        {
            ViewBag.CategoryId = id;
            ViewBag.ApiBaseUrl = _configuration["ApiSettings:BaseUrl"];
            return View();
        }
    }
}