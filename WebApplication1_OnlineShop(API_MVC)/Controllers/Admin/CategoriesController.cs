using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1_API_MVC_.Models;

namespace WebApplication1_API_MVC_.Controllers.Admin
{
    [Authorize(Roles = "Admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("categories")]
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("{id}")]
        public IActionResult Products(int? id)
        {
            return View();
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            //ViewBag.CategoryId = id;
            return View();
        }
        [HttpGet("delete/{id:int}")]
        public IActionResult Delete()
        {
            return View();
        }

    }
}
