using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class ChangePasswordDTO
    {
        [Required(ErrorMessage ="Please enter your password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }
        [Required]
        [StringLength(int.MaxValue, MinimumLength = 8, ErrorMessage = "Password must have at least 8 characters.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword",ErrorMessage ="Password is not same as new password")]
        public string ConfirmPassword { get; set; }

    }
}
