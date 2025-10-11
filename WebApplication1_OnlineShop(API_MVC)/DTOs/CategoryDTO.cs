using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.DTOs
{
    public class CategoryDTO
    {
        [Required]
        [RegularExpression(@"^(?![\W_]+$)(?!\d+$)[a-zA-Z0-9 .&',_-]+$", ErrorMessage = "Name must have characters.")]
        public string Name { get; set; }
        public int? CategoryParentId { get; set; }
    }
}
