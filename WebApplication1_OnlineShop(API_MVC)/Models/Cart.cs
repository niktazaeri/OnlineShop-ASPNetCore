using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.Models
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? CartIdentifier { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
    }
}
