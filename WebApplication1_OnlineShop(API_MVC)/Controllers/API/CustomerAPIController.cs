using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System.Threading.Tasks;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.Controllers.API
{
    [Route("api")]
    [ApiController]
    [Authorize(Roles ="Customer",AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    public class CustomerAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;

        public CustomerAPIController(UserManager<ApplicationUser> userManager , ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditProfile([FromBody] UpdateProfileDTO updateProfileDTO)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                user.PhoneNumber = updateProfileDTO.PhoneNumber;
                user.UserName = updateProfileDTO.UserName;
                user.FirstName = updateProfileDTO.Name;
                user.LastName = updateProfileDTO.LastName;
                user.Email = updateProfileDTO.Email;
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description);
                    return BadRequest(errors);
                }
                return Ok(new { message = "Your profile was updated successfully!" });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }
        }
        [HttpGet("order-history")]
        public async Task<IActionResult> OrderHistory()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var user_id = user.Id;
                var orders = _db.Orders.Where(o => o.UserId == user_id).Select(o => new
                {
                    o.CreatedAt,
                    o.AddressId
                }).ToList();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
    }
}
