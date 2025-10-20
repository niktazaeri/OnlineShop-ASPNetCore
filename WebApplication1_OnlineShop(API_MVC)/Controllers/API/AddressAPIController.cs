using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.Controllers.API
{
    [Authorize(Roles = "Customer", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/address")]
    [ApiController]
    public class AddressAPIController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressAPIController(ApplicationContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var addresses = _db.Addresses.Where(a => a.UserId == user.Id).Select(a =>
                new
                {
                    a.Id,
                    a.AddressText,
                    a.City,
                    a.Title,
                    a.ZipCode,
                }).ToList();

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }

        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AddressDTO addressDTO)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                var address = new Address
                {
                    Title = addressDTO.Title,
                    AddressText = addressDTO.AddressText,
                    City = addressDTO.City,
                    ZipCode = addressDTO.ZipCode,
                    UserId = user.Id
                };
                _db.Addresses.Add(address);
                _db.SaveChanges();
                return Created("", new { message = "new address was created." });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }
        }
        [HttpPut("edit/{id:int}")]
        public IActionResult Edit([FromBody] AddressDTO addressDTO, int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound(new { message = "address not found!" });
            }
            var address = _db.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
            {
                return NotFound(new { message = "address not found!" });

            }
            if (ModelState.IsValid)
            {
                address.AddressText = addressDTO.AddressText;
                address.City = addressDTO.City;
                address.ZipCode = addressDTO.ZipCode;

                _db.Addresses.Update(address);
                _db.SaveChanges();

                return Ok(new { message = "address updated successfully!" });
            }
            else
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

        }
        [HttpGet("{id:int}")]
        public IActionResult GetAddress(int id)
        {
            if (id == 0 || id == null)
            {
                return NotFound(new { message = "address not found!" });
            }
            var address = _db.Addresses.FirstOrDefault(a => a.Id == id);
            if (address == null)
            {
                return NotFound(new { message = "address not found!" });

            }
            else
            {
                return Ok(address);
            }
        }
        [HttpDelete("delete/{id:int}")]
        public IActionResult DeleteAddress(int id)
        {
            if(id==0 || id == null)
            {
                return NotFound(new { message = "address not found!" });
            }
            var address = _db.Addresses.FirstOrDefault(a => a.Id==id);
            if(address == null)
            {
                return NotFound(new { message = "address not found!" });
            }
            _db.Addresses.Remove(address);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
