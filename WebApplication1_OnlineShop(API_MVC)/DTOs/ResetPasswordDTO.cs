using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class ResetPasswordDTO
    {
        [Required , DataType(DataType.EmailAddress,ErrorMessage ="Invalid email format") , EmailAddress(ErrorMessage ="Invalid email")]
        public string Email { get; set; }
        [Required, DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 8, ErrorMessage = "Password must have at least 8 characters.")]
        public string NewPassword { get; set; }
        [Required, DataType(DataType.Password),Compare("NewPassword",ErrorMessage ="Passwords do not match") ]
        public string ConfirmPassword { get; set; }
        public string Token { get; set; }
    }
}
