using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.Identity;
using WebApplication1_API_MVC_.Services;
namespace WebApplication1_API_MVC_
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();

            builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
                options.TokenLifespan = TimeSpan.FromHours(1));

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            });



            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IEmailSender, EmailService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();


            // JWT config
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie( options =>
            {
                //options.Cookie.Name = "MyProjectCookie";
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // HTTPS only
                options.SlidingExpiration = true;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"])),
                ClockSkew = TimeSpan.Zero
                };
            });

            //builder.Services.AddDistributedMemoryCache();
            //builder.Services.AddSession(options =>
            //{
            //    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
            //    options.Cookie.HttpOnly = true; // Ensures the session cookie is accessible only by the server
            //    options.Cookie.IsEssential = true; // Required for GDPR compliance
            //});
            //builder.Services.AddControllersWithViews();

            builder.Services.AddAuthorization();

            var app = builder.Build();


            //static async Task SeedRolesAsync(IServiceProvider serviceProvider)
            //{
            //    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            //    string[] roleNames = { "Admin", "Customer" };
            //    foreach (var roleName in roleNames)
            //    {
            //        if (!await roleManager.RoleExistsAsync(roleName))
            //        {
            //            await roleManager.CreateAsync(new IdentityRole(roleName));
            //        }
            //    }
            //    var admins = new List<(string Name, string LastName, string UserName, string Email, string Password)>
            //    {
            //        ("?????","?????","niktazaeriadmin","niktazaeri1@gmail.com","12345678"),
            //        ("??? ?????","?????","nikafaridzaeriadmin","nikafarid@gmail.com","12345678")
            //    };
            //    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            //    foreach (var admin in admins)
            //    {
            //        var user = await userManager.FindByEmailAsync(admin.Email);
            //        if (user == null)
            //        {
            //            var new_user = new ApplicationUser
            //            {
            //                FirstName = admin.Name,
            //                LastName = admin.LastName,
            //                UserName = admin.UserName,
            //                Email = admin.Email
            //            };
            //            var created_new_user = await userManager.CreateAsync(new_user, admin.Password);
            //            if (created_new_user.Succeeded)
            //            {
            //                await userManager.AddToRoleAsync(new_user, "Admin");
            //            }

            //        }
            //    }
            //}

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                string[] roleNames = { "Admin", "Customer" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }
            }

            using (var scope = app.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

                var admins = new List<(string Name, string LastName, string UserName, string Email, string Password)>
                {
                    ("?????","?????","niktazaeriadmin","niktazaeri1@gmail.com","12345678"),
                    ("??? ?????","?????","nikafaridzaeriadmin","nikafarid@gmail.com","12345678")
                };
                foreach (var admin in admins)
                {
                    var user = await userManager.FindByEmailAsync(admin.Email);
                    if (user == null)
                    {
                        var new_user = new ApplicationUser
                        {
                            FirstName = admin.Name,
                            LastName = admin.LastName,
                            UserName = admin.UserName,
                            Email = admin.Email,
                            ProfilePicture = "default-avatar-icon-of-social-media-user-vector.jpg"

                        };
                        var created_new_user = await userManager.CreateAsync(new_user, admin.Password);
                        if (created_new_user.Succeeded)
                        {

                            await userManager.AddToRoleAsync(new_user, "Admin");
                        }

                    }
                    if (user!= null && user.ProfilePicture == null) {
                        user.ProfilePicture = "default-avatar-icon-of-social-media-user-vector.jpg";
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }
    }
}


