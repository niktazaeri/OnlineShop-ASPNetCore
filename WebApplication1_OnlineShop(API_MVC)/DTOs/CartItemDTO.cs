using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1_API_MVC_.Models;

namespace WebApplication1_API_MVC_.DTOs
{
    public class CartItemDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be more than 0.")]
        public double Price { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int CartId { get; set; }
    }
}
