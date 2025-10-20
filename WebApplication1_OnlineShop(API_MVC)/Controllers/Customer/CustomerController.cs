using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_API_MVC_.Controllers.Customer
{
    [Authorize(Roles ="Customer",AuthenticationSchemes =CookieAuthenticationDefaults.AuthenticationScheme)]
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
        [HttpGet("me/addresses")]
        public IActionResult Addresses()
        {
            return View();
        }
        [HttpGet("me/change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [HttpGet("me/order-history")]
        public IActionResult OrdersHistoy()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Cart()
        {
            return View();
        }
    }
}
