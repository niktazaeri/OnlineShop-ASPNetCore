using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1_API_MVC_.Controllers
{
    [Route("Account")]
    public class AccountController : Controller
    {
        [HttpGet("sign-up")]
        public IActionResult Register()
        {
            return View();
        }
        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }
        [HttpGet("logout")]
        public IActionResult LogOut()
        {
            return View();
        }
        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpGet("reset-password")]
        public IActionResult ResetPassword()
        {
            return View();
        }
    }
}
