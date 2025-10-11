using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_API_MVC_.Controllers.Customer
{
    [Authorize]
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
            return View("Index","Home");
        }
        [HttpGet("me")]
        public IActionResult Profile()
        {
            return View();
        }
    }
}
