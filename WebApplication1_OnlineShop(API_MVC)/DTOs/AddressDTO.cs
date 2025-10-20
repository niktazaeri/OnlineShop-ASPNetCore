using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class AddressDTO
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string AddressText { get; set; }
        [Required]
        [RegularExpression(@"^[\p{IsArabic}A-Za-z_'\s]+$",
         ErrorMessage = "Not valid city name.")]
        public string City { get; set; }
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Not valid! zip code must have 10 digits.")]
        public long ZipCode { get; set; }
    }
}
