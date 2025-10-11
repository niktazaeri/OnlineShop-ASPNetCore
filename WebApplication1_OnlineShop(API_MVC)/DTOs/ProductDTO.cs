using Microsoft.DotNet.Scaffolding.Shared;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class ProductDTO
    {
        [Required]
        [RegularExpression(@"^(?![\W_]+$)(?!\d+$)[a-zA-Z0-9 .&',_-]+$", ErrorMessage = "Name must have characters.")]
        public string Name { get; set; }
        [Required]
        [StringLength(250, ErrorMessage = "Description must have maximum 250 lenghth.")]
        public string Description { get; set; }
        [Required]
        public double Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string? PictureName { get; set; }
        public IFormFile? PictureFile { get; set; }

        public int CategoryId { get; set; }
    }
}
