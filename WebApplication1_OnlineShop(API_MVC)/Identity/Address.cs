using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.Identity
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string AddressText { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z\s\-]{1,40}$",
         ErrorMessage = "Not valid city name.")]
        public string City { get; set; }
        [Required]
        [RegularExpression(@"^\d{10}$", ErrorMessage ="Not valid! zip code must have 10 digits.")]
        public int ZipCode { get; set; }

        public string UserId { get; set; }

        public ApplicationUser User { get; set; }
    }
}
