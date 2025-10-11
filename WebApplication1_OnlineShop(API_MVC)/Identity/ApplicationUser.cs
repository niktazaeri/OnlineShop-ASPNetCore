using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.Identity
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50 , MinimumLength =2 , ErrorMessage ="Name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid Name.")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid last name.")]
        public string LastName { get; set; }
        [Required]
        public string ProfilePicture { get; set; }
        public int AddressId { get; set; }

        public ICollection<Address> Addresses { get; set; }

    }
}
