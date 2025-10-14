using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1_API_MVC_.Models
{
    public class CartItem
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        [ForeignKey("Cart")]
        public int CartId { get; set; }
        public Cart Cart { get; set; }
    }
}
