using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class ForgotPasswordDTO
    {
        [Required]
        [EmailAddress(ErrorMessage ="Invalid email format.")]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}
