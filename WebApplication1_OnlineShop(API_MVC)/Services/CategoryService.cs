using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.Models;

namespace WebApplication1_API_MVC_.Services
{
    public interface ICategoryService
    {
        List<string> GetParentCategories(Category category);
    }

    public class CategoryService : ICategoryService
    {


        private readonly ApplicationContext _db;

        public CategoryService(ApplicationContext db)
        {
            _db = db;
        }
        public List<string> GetParentCategories(Category category)
        {
            var categoryName = category.Name;
            List<string> category_parents_name = new List<string>();
            while (category.CategoryParentId != null)
            {
                var categoryParent = _db.Categories.FirstOrDefault(cp => cp.Id == category.CategoryParentId);
                category_parents_name.Add(categoryParent.Name);
                category = categoryParent;
            }
            return category_parents_name;
        }
    }
}
