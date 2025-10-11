using Humanizer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Models;
using WebApplication1_API_MVC_.Services;

namespace WebApplication1_API_MVC_.Controllers.API
{
    [Authorize(Roles ="Admin",AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/products")]
    [ApiController]
    public class ProductsAPIController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly IFileService _fileService;
        private readonly ICategoryService _categoryService;

        public ProductsAPIController(ApplicationContext db , IFileService fileService, ICategoryService categoryService)
        {
            _db = db;
            _fileService = fileService;
            _categoryService = categoryService;
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult GetProducts() {
            var products = _db.Products.Select(p => new
            {
                p.Id,
                p.Name,
                p.Description,
                p.Price,
                p.CategoryId,
                p.Quantity,
                p.PictureName,
            }).ToList();
            List<string> imagePaths = new List<string>();
            for (int i=0; i<products.Count; i++) {
                string path = "";
                if (products[i].PictureName == "missing-picture-page-website-design-600nw-1552421075.webp")
                {
                    path = "";
                }
                else
                {
                    var category = _db.Categories.FirstOrDefault(c => c.Id == products[i].CategoryId);
                    var category_name = category.Name;
                    var category_parents_name = _categoryService.GetParentCategories(category);
                    if (category_parents_name.Count > 0)//if category has parents
                    {
                        category_parents_name.Reverse();
                        foreach (var cat in category_parents_name)
                        {
                            path += $"{cat}/";
                        }
                        path += category_name;
                    }
                    else
                    {
                        path += category_name;
                    }
                }
                imagePaths.Insert(i, path);
            }
            return Ok(new { products, imagePaths });
        }
        [AllowAnonymous]
        [HttpGet("{id}")]
        public IActionResult GetProduct(int? id) {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            var category_id = _db.Products.Where(p => p.Id == id).Select(p => p.CategoryId).FirstOrDefault();
            var category = _db.Categories.FirstOrDefault(c => c.Id == category_id);
            var categoryName = category.Name;
            var category_parents_name = new List<string>();
            if (product.PictureName != "missing-picture-page-website-design-600nw-1552421075.webp")
            {
                category_parents_name = _categoryService.GetParentCategories(category);
            }
            else
            {
                category_parents_name = [];
            }
            return Ok(new { categoryName, product = new { product.Name, product.Description, product.Price, product.Quantity, product.PictureName }, category_parents_name });
        }
        [HttpPost("create")]
        public IActionResult Create([FromForm] ProductDTO dto)
        {
            if (ModelState.IsValid)
            {
                var category = _db.Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
                var categoryName = category.Name;
                var category_parents_name = _categoryService.GetParentCategories(category);
                string pictureName;
                if (dto.PictureFile != null)
                {
                    pictureName = _fileService.SaveProductImage(dto.PictureFile, category_parents_name, categoryName);
                }
                else
                    pictureName = "missing-picture-page-website-design-600nw-1552421075.webp";
                var product = new Products
                {
                    CategoryId = dto.CategoryId,
                    Description = dto.Description,
                    Price = dto.Price,
                    Name = dto.Name,
                    Quantity = dto.Quantity,
                    PictureName = pictureName
                };
                _db.Products.Add(product);
                _db.SaveChanges();
                return Created();
            }
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        }

        [HttpPut("edit/{id:int}")]
        public IActionResult Edit(int? id ,[FromForm] ProductDTO dto)
        {
            try
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                var existingProduct = _db.Products.Find(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }
                if (ModelState.IsValid)
                {
                    existingProduct.Description = dto.Description;
                    existingProduct.Price = dto.Price;
                    existingProduct.Quantity = dto.Quantity;
                    existingProduct.Name = dto.Name;
                    existingProduct.CategoryId = dto.CategoryId;
                    var category = _db.Categories.FirstOrDefault(c => c.Id == dto.CategoryId);
                    var category_name = category.Name;
                    //category parents
                    var category_parents_name = _categoryService.GetParentCategories(category);
                    if (dto.PictureFile != null)
                    {
                        var pictureName = _fileService.SaveProductImage(dto.PictureFile, category_parents_name, category_name);
                        existingProduct.PictureName = pictureName;
                    }
                    _db.Products.Update(existingProduct);
                    _db.SaveChanges();
                    return Ok();
                }
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpDelete("delete/{id:int}")]
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var existingProduct = _db.Products.Find(id);
            if (existingProduct == null)
            {
                return NotFound();
            }
            _db.Products.Remove(existingProduct);
            _db.SaveChanges();
            return NoContent();
            
        }

        
    }
}
