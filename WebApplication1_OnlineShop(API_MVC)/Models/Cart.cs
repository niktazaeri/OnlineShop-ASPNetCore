using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
