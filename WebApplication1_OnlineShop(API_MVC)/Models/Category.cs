using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^(?![\W_]+$)(?!\d+$)[a-zA-Z0-9 .&',_-]+$", ErrorMessage = "Name must have characters.")]
        public string Name { get; set; }

        public int? CategoryParentId { get; set; }
        public Category ParentCategory { get; set; }
        public ICollection<Products> Products { get; set; }
        public ICollection<Category> SubCategory { get; set; }
    }
}
