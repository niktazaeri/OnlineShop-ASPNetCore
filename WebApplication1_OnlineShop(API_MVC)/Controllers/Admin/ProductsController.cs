using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_API_MVC_.Controllers.Admin
{
    [Authorize(Roles = "Admin",AuthenticationSchemes =CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("products")]
    public class ProductsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("{id}")]
        public IActionResult Details(int? id) // Products/{id}
        {
            return View();
        }
        [HttpGet("create")]
        public IActionResult Create() 
        {
            return View();
        }
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit()
        {
            return View();
        }
        [HttpGet("delete/{id:int}")]
        public IActionResult Delete()
        {
            return View();
        }
    }
}
