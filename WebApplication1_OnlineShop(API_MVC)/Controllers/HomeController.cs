using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApplication1_API_MVC_.Identity;
using WebApplication1_API_MVC_.Models;

namespace WebApplication1_API_MVC_.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger , UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
        
        public IActionResult Index()
        {
            var user =HttpContext.User;
            if (user.Identity.IsAuthenticated == false)
            {
                return View();
            }
            var user_role = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.Role);
            if (user_role.Value == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
