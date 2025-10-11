using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Models;
using WebApplication1_API_MVC_.Services;

namespace WebApplication1_API_MVC_.Controllers.API
{
    [Authorize(Roles ="Admin",AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/categories")]
    [ApiController]
    public class CategoriesAPIController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly ICategoryService _categoryService;
        public CategoriesAPIController(ApplicationContext db , ICategoryService category)
        {
            _db = db;
            _categoryService = category;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetCategoriesList()
        {
            var categories = _db.Categories
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.CategoryParentId
                })
                .ToList();

            return Ok(categories);
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetCategory(int id) {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _db.Categories.Include(c => c.SubCategory).Include(c => c.ParentCategory).FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            var categoryParent = category.ParentCategory;
            List<int> categoryParentId_list = new List<int>();
            while(categoryParent != null)
            {
                categoryParentId_list.Add(categoryParent.Id);
                if (categoryParent.CategoryParentId == null)
                    break;
                categoryParent = _db.Categories.Include(c => c.ParentCategory).FirstOrDefault(c => c.Id == categoryParent.CategoryParentId);
            }
            return Ok(new
            {
                Category = new
                {
                    category.Id,
                    category.Name,
                    category.CategoryParentId
                },
                CategoryParentId = categoryParentId_list,
                SubCategory = category.SubCategory.Select(sc => new
                {
                    sc.Id,
                    sc.Name
                }).ToList()
            });
        }
        [AllowAnonymous]
        [HttpGet("{id}/products")]
        public IActionResult GetCategoriesProducts(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound(new {message = "category not found"});
            }

            var category = _db.Categories.Include(c => c.SubCategory).FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound(new {message = "category not found"});
            }

            //extract subcategories

            List<int> subcats_ids = new List<int>();

            List<Category> subcat_list = new List<Category>();

            subcat_list = category.SubCategory.ToList();

            List<Products> products = new List<Products>();

            if (subcat_list.Count == 0)
            {
                products = _db.Products.Where(p => p.CategoryId == id).Select(p => new Products
                {
                    Id = p.Id,
                    Name =p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    Description = p.Description,
                    CategoryId = p.CategoryId,
                    PictureName = p.PictureName
                }).ToList();
            }
            else
            {
                while (subcat_list.Count > 0)
                {
                    foreach (var subcategory in subcat_list)
                    {
                        subcats_ids.Add(subcategory.Id);

                    }
                    var ids = subcat_list.Select(sc => sc.Id).ToList();
                    subcat_list = _db.Categories.Include(c => c.SubCategory).Where(c => ids.Contains(c.CategoryParentId ?? 0)).ToList();

                }
                products = _db.Products.Where(p => subcats_ids.Contains(p.CategoryId)).Select(
                p => new Products
                {
                    Id = p.Id,
                    CategoryId =p.CategoryId,
                    Name = p.Name,
                    Price = p.Price,
                    Quantity = p.Quantity,
                    PictureName = p.PictureName,
                    Description = p.Description,
                }).ToList();
            }    
            //extracting product image path
            List<string> imagePaths = new List<string>();
            string imagePath = "";
            for(int i = 0; i < products.Count; i++)
            {
                if (products[i].PictureName != "missing-picture-page-website-design-600nw-1552421075.webp")
                {
                    var product_category = _db.Categories.FirstOrDefault(c => c.Id == products[i].CategoryId);
                    var product_category_name = product_category.Name;
                    var product_category_parents_name = _categoryService.GetParentCategories(product_category);
                    if (product_category_parents_name.Count > 0)
                    {
                        product_category_parents_name.Reverse();
                        foreach(var cat in product_category_parents_name)
                        {
                            imagePath += cat + "/";
                        }
                        imagePath += product_category_name;
                        imagePaths.Insert(i,imagePath);
                        imagePath = "";
                    }
                }
                else
                {
                    imagePath = "";
                    imagePaths.Insert(i,imagePath);
                }   
            }
            return Ok(new { products, imagePaths });
        }

        [HttpPost("create")]
        public IActionResult Create([FromBody] CategoryDTO dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var category = new Category { Name = dto.Name , CategoryParentId = dto.CategoryParentId };
                    _db.Categories.Add(category);
                    _db.SaveChanges();
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPut("edit/{id:int}")]
        public IActionResult Edit(int id, [FromBody] CategoryDTO dto)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var existingCategory = _db.Categories.Find(id);
                    if (existingCategory == null)
                    {
                        return NotFound($"Category with ID {id} not found.");
                    }

                    existingCategory.Name = dto.Name;
                    existingCategory.CategoryParentId = dto.CategoryParentId;
                    _db.SaveChanges();
                    return Ok();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpDelete("delete/{id:int}")]
        public IActionResult Delete(int? id) { 
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _db.Categories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            _db.Categories.Remove(category);
            _db.SaveChanges();
            return Ok();
        }
        
    }
}
