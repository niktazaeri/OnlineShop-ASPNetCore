using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1_API_MVC_.Identity;

namespace WebApplication1_API_MVC_.Models
{
    public class Order
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy HH:mm}")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        [ForeignKey("Address")]
        public int AddressId { get; set; }
        public Address Address { get; set; }
    }
}
