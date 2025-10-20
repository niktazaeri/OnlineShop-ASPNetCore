using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Identity;
using WebApplication1_API_MVC_.Services;

namespace WebApplication1_API_MVC_.Controllers.API
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IFileService _fileService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ApplicationContext db ,UserManager<ApplicationUser> userManager , IConfiguration configuration ,
            SignInManager<ApplicationUser> signInManager, IFileService fileService , IEmailSender emailSender ,
            ILogger<AuthController> logger)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _config = configuration;
            _fileService = fileService;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser
                    {
                        FirstName = registerDTO.Name,
                        LastName = registerDTO.LastName,
                        Email = registerDTO.Email,
                        PhoneNumber = registerDTO.PhoneNumber.ToString(),
                        UserName = registerDTO.Username
                    };
                    var result = await _userManager.CreateAsync(user, registerDTO.Password);
                    if (result.Succeeded)
                    {
                        foreach (var address in registerDTO.Addresses)
                        {
                            var user_address = new Address
                            {
                                Title = address.Title,
                                City = address.City,
                                AddressText = address.AddressText,
                                ZipCode = address.ZipCode
                            };
                            user_address.UserId = user.Id;
                            _db.Addresses.Add(user_address);
                        }

                        var role = await _userManager.AddToRoleAsync(user, "Customer");

                        var token = await GenerateJwtToken(user);
                        //_db.Tokens.Add(
                        //    new UserToken
                        //    {
                        //        UserId = user.Id,
                        //        Token = token,
                        //        Expiration = DateTime.UtcNow.AddMinutes(15),
                        //        IsRevoked = false
                        //    }
                        //    );
                        
                        var jwtSettings = _config.GetSection("JwtSettings");
                        Response.Cookies.Append("accessToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]))
                        });
                        var refresh_token = RefreshToken.GenerateRefreshToken(user);
                        await _db.RefreshTokens.AddAsync(refresh_token);
                        await _db.SaveChangesAsync();
                        Response.Cookies.Append("refreshToken", refresh_token.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            Secure = true,
                            Expires = refresh_token.Expiration
                        });
                        return Created("",
                        new
                        {
                            message = $"{registerDTO.Name} has registered successfully.",
                            token = token

                        });
                    }
                    else if (!result.Succeeded)
                    {
                        var errors = result.Errors.Select(e => e.Description).ToList();
                        return BadRequest(errors);
                    }
                }
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new { errors = modelErrors });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO )
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByNameAsync(loginDTO.Username);
                    if (user != null && await _userManager.CheckPasswordAsync(user, loginDTO.Password))
                    {
                        //var loginResult = await _signInManager.PasswordSignInAsync(user, loginDTO.Password,true,false);
                        var token = await GenerateJwtToken(user);
                            //var login_token = new UserToken
                            //{
                            //    UserId = user.Id,
                            //    Token = token,
                            //    Expiration = DateTime.UtcNow.AddMinutes(15),
                            //    IsRevoked = false
                            //};
                            //_db.Tokens.Add(login_token);
                            var refresh_token = RefreshToken.GenerateRefreshToken(user);
                            _db.RefreshTokens.Add(refresh_token);
                            await _db.SaveChangesAsync();

                            var role = await _userManager.GetRolesAsync(user);
                            await CookieConfiguration(user,role);
                        var jwtSettings = _config.GetSection("JwtSettings");
                        Response.Cookies.Append("accessToken", token, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]))
                        });

                        Response.Cookies.Append("refreshToken", refresh_token.Token, new CookieOptions
                        {
                            HttpOnly = true,
                            SameSite = SameSiteMode.Strict,
                            Secure = true,
                            Expires = refresh_token.Expiration
                        });

                        return Ok(new { token,user_role = role });
                        

                    }
                    else if(user == null)
                    {
                        return Unauthorized(new
                        {
                            error = "Invalid username or password"
                        });
                    }
                }
                var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return Unauthorized(new { errors = modelErrors });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
                throw;
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("who-am-i")]
        public async Task<IActionResult> WhoAmI()
        {
            return Ok(User.Claims.Select(c => new { c.Type, c.Value }));
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString(); //get access token

                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest("Missing Authorization header");
                }
                token = token.Replace("Bearer ", "");

                if (!token.IsNullOrEmpty())
                {
                    var user = await _userManager.GetUserAsync(HttpContext.User);
                    
                    var refresh_token = _db.RefreshTokens.Where(rt => rt.UserId == user.Id && rt.IsRevoked == false);

                    if (refresh_token.Count() == 0)
                    {
                        return BadRequest("user has already loggedout");
                    }

                    foreach (var rt in refresh_token)
                    {
                        rt.IsRevoked = true;
                    }
                    await _db.SaveChangesAsync();
                    Response.Cookies.Delete("refreshToken");
                    Response.Cookies.Delete("accessToken");
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return Ok($"{user.UserName} logged out successfully.");
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("get-user")]
        public async Task<IActionResult> GetUser()
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return NotFound("user not found");
            }
            return Ok(user);
        }


        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-profile")]
        public async Task<IActionResult> EditInfos([FromForm] UpdateProfileDTO dto)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    return NotFound(new { error = "user not found" });
                }
                if (ModelState.IsValid)
                {
                    user.PhoneNumber = dto.PhoneNumber;
                    user.UserName = dto.UserName;
                    user.Email = dto.Email;
                    user.FirstName = dto.Name;
                    user.LastName = dto.LastName;
                    if(dto.ProfileImage == null)
                    {
                        user.ProfilePicture = "default-avatar-icon-of-social-media-user-vector.jpg";
                    }else
                    {
                        user.ProfilePicture = _fileService.SaveProfileImage(dto.ProfileImage);
                    }
                    if (dto.ProfilePicture != null) {
                        _fileService.RemoveProfileImage(dto.ProfilePicture);
                    }
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();
                    return Ok(user);
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(new {error = ex.Message});
                throw;
            }
        }
        [Authorize(Roles = "Admin",AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("remove-profile-photo")]
        public async Task<IActionResult> RemoveProfilePhoto([FromBody] UpdateProfileDTO dto)
        {
            var user = await _userManager.GetUserAsync(HttpContext.User);
            if (user == null)
            {
                return NotFound(new { error = "user not found" });
            }
            if (dto.ProfilePicture == null) {
                return BadRequest();
            }
            if (dto.ProfilePicture == "default-avatar-icon-of-social-media-user-vector.jpg")
            {
                return BadRequest();
            }
            //var defaultProfilePic = _fileService.RemoveProfileImage(dto.ProfilePicture);
            //user.ProfilePicture = defaultProfilePic;
            var defaultProfilePic = "default-avatar-icon-of-social-media-user-vector.jpg";
            user.ProfilePicture = defaultProfilePic;
            //_db.Users.Update(user);
            //await _db.SaveChangesAsync();
            return Ok(new { profilePicture = defaultProfilePic , previousProfilePic = dto.ProfilePicture });

        }
        [Authorize(Roles = "Admin,Customer",AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO dto )
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null) { return NotFound(new { errors = new[] { "No user found" } }); }
                if (ModelState.IsValid)
                {
                    var check_current_password = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
                    if (check_current_password == false)
                    {
                        return BadRequest(new { errors = new []{ "Invalid password." } });
                    }
                    var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                    if (!result.Succeeded)
                    {
                        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                    }
                    _db.Users.Update(user);
                    await _db.SaveChangesAsync();

                    string body = $@"
                            Hello {user.FirstName},

                            Your password was successfully changed on {DateTime.UtcNow:f} (UTC).
                            If you did not make this change, please contact support immediately.

                            or reset you password via this link: https://localhost:44361/Account/forgot-password

                            Thanks,
                            Your Security Team
                            ";
                    await _emailSender.ChangePasswordNotification(user.Email, "Your password has been changed!", body);

                    return Ok();
                }
                return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(dto.Email);
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    if (!token.IsNullOrEmpty())
                    {
                        string resetPasswordUrl = $"https://localhost:44361/Account/reset-password?email={user.Email}&token={WebUtility.UrlEncode(token)}";
                        string body = $@"
            Hello {user.FirstName},

            You requested a password reset. Click the link below to reset your password:

            {resetPasswordUrl}

            If you didn't request this, ignore this email.

            Thanks,
            Your Security Team
        ";
                        await _emailSender.ResetPasswordLink(user.Email, "reset your password", body);
                    }
                    return Accepted(new { message = $"We sent an email to {dto.Email}, Please check it out." });
                }
                else
                {
                    return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
            
        }
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _userManager.FindByEmailAsync(dto.Email);
                    if (user == null) { return NotFound(new { error = "User not found" }); }
                    var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
                    if (result.Succeeded)
                    {
                        string body = $@"
                            Hello {user.FirstName},

                            Your password was successfully changed on {DateTime.UtcNow:f} (UTC).
                            If you did not make this change, please contact support immediately.

                            or reset you password via this link: https://localhost:44361/Account/forgot-password/

                            Thanks,
                            Your Security Team
                            ";
                        await _emailSender.ChangePasswordNotification(user.Email, "Your password has been changed!", body);
                        return Ok(new { message = "Password has been reset." });
                    }
                    else
                        return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
                }
                else
                {
                    return BadRequest(new { errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList() });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> CheckRefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();
            var refresh_token_db = await _db.RefreshTokens //refresh token existed in database
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (refresh_token_db == null)
                return NotFound(new { error = "Refresh token not found" });

            if (refresh_token_db.Expiration <= DateTime.UtcNow)
                return Unauthorized(new { error = "Refresh token expired" });

            if (refresh_token_db.IsRevoked)
                return Unauthorized(new { error = "Refresh token has already been used" });

            var user = refresh_token_db.User;
            refresh_token_db.IsRevoked = true;

            var new_refresh_token = RefreshToken.GenerateRefreshToken(user);

            Response.Cookies.Append("refreshToken", new_refresh_token.Token, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Strict,
                Secure = true,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            //var old_jwt_token = await _db.Tokens.Include(ot => ot.User).FirstOrDefaultAsync(ot => ot.UserId == user.Id);
            //if (old_jwt_token != null)
            //{
            //    old_jwt_token.IsRevoked = true;
            //}

            var new_jwtToken = await GenerateJwtToken(user);
            //var new_jwtToken = new UserToken
            //{
            //    UserId = user.Id,
            //    Token = jwtToken,
            //    Expiration = DateTime.UtcNow.AddMinutes(15),
            //    IsRevoked = false
            //};
            //_db.Tokens.Add(new_jwtToken);
            _db.RefreshTokens.Add(new_refresh_token);
            await _db.SaveChangesAsync();

            var jwtSettings = _config.GetSection("JwtSettings");
            Response.Cookies.Append("accessToken", new_jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]))
            });

            return Ok(new
            {
                token = new_jwtToken
                //refresh_token = new_refresh_token.Token,
                //expiration = new_refresh_token.Expiration
            });
        }


        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                // Required for ASP.NET Identity to map user
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id),          // important for GetUserAsync
                new Claim(ClaimTypes.Name, user.UserName),              // User.Identity.Name
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Optional: Add user claims from Identity if you have any stored
            var userClaims = await _userManager.GetClaimsAsync(user);
            claims.AddRange(userClaims);

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task CookieConfiguration(ApplicationUser user , IList<string> user_role)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Role,user_role[0])
            };
            var identity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var props = new AuthenticationProperties
            {
                IsPersistent = true, // keeps cookie across browser restarts
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7), // cookie lifetime
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
            
        }

    }
}
