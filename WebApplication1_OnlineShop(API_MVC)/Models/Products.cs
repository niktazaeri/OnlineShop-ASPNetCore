using System.ComponentModel.DataAnnotations;

namespace WebApplication1_API_MVC_.Models
{
    public class Products
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^(?![\W_]+$)(?!\d+$)[a-zA-Z0-9 .&',_-]+$", ErrorMessage = "Name must have characters.")]
        public string Name { get; set; }
        [Required]
        [StringLength(250,ErrorMessage ="Description must have maximum 250 lenghth.")]
        public string Description { get; set; }
        [Required]
        [Range(1,double.MaxValue,ErrorMessage ="Price must be more than 0.")]
        public double Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        public string PictureName { get; set; }
        public int CategoryId { get; set; }

        public Category Category { get; set; }
    }
}
