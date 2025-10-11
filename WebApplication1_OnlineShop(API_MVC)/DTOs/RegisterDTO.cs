using System.ComponentModel.DataAnnotations;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.DTOs
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid Name.")]
        public string Name { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid last name.")]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Not a valid phone number")]
        public string PhoneNumber { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Username must has at least 4 charachters and maximum 15 characters.")]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue,MinimumLength = 8, ErrorMessage = "Password must have at least 8 characters.")]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        public string ConfirmedPassword { get; set; }
        public List<AddressDTO> Addresses { get; set; }

    }
}
