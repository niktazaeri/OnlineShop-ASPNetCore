using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_API_MVC_.Controllers.Admin
{
    [Authorize(Roles = "Admin", AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [Route("Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("me")]
        public IActionResult Profile()
        {
            return View();
        }
        [HttpGet("change-password")]
        public IActionResult ChangePassword()
        {
            return View();
        }
        
    }
}
