using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class UpdateProfileDTO
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid last name.")]
        public string? Name { get; set; }
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must has at least 2 charachters and maximum 50 characters.")]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z0-9_'\s]+$",
         ErrorMessage = "Not valid last name.")]
        public string? LastName { get; set; }
        public string? Email { get; set; }
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Not a valid phone number")]
        public string? PhoneNumber { get; set; }
        [StringLength(15, MinimumLength = 4,
        ErrorMessage = "Username must has at least 4 charachters and maximum 15 characters.")]
        public string? UserName { get; set; }
        public string? ProfilePicture { get; set; }
        public IFormFile? ProfileImage { get; set; }
        //for users addresses
        public List<AddressDTO>? Addresses { get; set; }

    }
}
