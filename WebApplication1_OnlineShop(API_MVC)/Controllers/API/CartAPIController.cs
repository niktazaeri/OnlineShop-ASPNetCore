using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.Tasks;
using WebApplication1_API_MVC_.Context;
using WebApplication1_API_MVC_.DTOs;
using WebApplication1_API_MVC_.Identity;
using WebApplication1_API_MVC_.Models;

namespace WebApplication1_API_MVC_.Controllers.API
{
    //[Authorize(Roles ="Customer",AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [AllowAnonymous]
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationContext _db;

        public CartAPIController(UserManager<ApplicationUser> userManager , ApplicationContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                Cart cart = null;
                string cartIdentifier = Request.Cookies["CartIdentifier"];
                if (HttpContext.User.Identity.IsAuthenticated == false) //guest
                {
                    if (!cartIdentifier.IsNullOrEmpty())//guest added to cart before
                    {
                        var carts = _db.Carts.Where(c => c.CreatedAt < DateTime.UtcNow.AddDays(-7)).ToList();
                        if (carts.Count() > 0) //if cart is expired
                        {
                            foreach (var item in carts)
                            {
                                _db.Carts.Remove(item);
                            }
                            _db.SaveChanges();
                        }
                        else
                        {
                            cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                            return Ok(cart);
                        }
                    }
                }
                else // logged in user
                {
                    var user_id = HttpContext.User.Claims.ToList()[1].Value;
                    if (!cartIdentifier.IsNullOrEmpty())//added to cart before and now we should merge guest's cart with user's cart
                    {
                        cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                        var user_last_cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user_id);
                        if (user_last_cart != null)//user has cart before
                        {
                            var user_last_cartItems = user_last_cart.CartItems.ToList();
                            var cart_items = cart.CartItems.ToList();//items in guest's cart
                            foreach(var item in cart_items)
                            {
                                foreach(var userCartItem in user_last_cartItems)
                                {
                                    if (item.Name == userCartItem.Name)
                                    {
                                        var existingItem = user_last_cartItems.FirstOrDefault(i => i.Name == item.Name && i.CartId == userCartItem.CartId);
                                        existingItem.Quantity = existingItem.Quantity + item.Quantity;
                                        _db.CartItems.Remove(item);
                                        _db.CartItems.Update(existingItem);
                                        _db.SaveChanges();
                                    }
                                    else
                                    {
                                        item.CartId = user_last_cart.Id;
                                        _db.CartItems.Update(item);
                                    }
                                }
                                
                            }
                            _db.Carts.Remove(cart);
                            _db.SaveChanges();
                        }
                        else
                        {
                            cart.CartIdentifier = "0"; //not guest anymore
                            cart.UserId = user_id;

                            _db.Carts.Update(cart);
                            _db.SaveChanges();
                        }
                        Response.Cookies.Delete("CartIdentifier");
                    }
                    else
                    {
                        cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user_id);
                    }
                }
                return Ok(cart);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPost("add-to-cart")]
        public async Task<IActionResult> AddToCart([FromBody] CartItemDTO cartItemDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    string cartIdentifier = Request.Cookies["CartIdentifier"];
                    if (!HttpContext.User.Identity.IsAuthenticated)//guest
                    {
                        if (cartIdentifier.IsNullOrEmpty())//user has never added to cart
                        {
                            cartIdentifier = Guid.NewGuid().ToString();
                            Response.Cookies.Append("CartIdentifier", cartIdentifier, new CookieOptions
                            {
                                HttpOnly = true,
                                SameSite = SameSiteMode.Strict,
                                Secure = true,
                                Expires = DateTime.UtcNow.AddDays(7)
                            });
                            //create a new cart
                            var cart = new Cart();
                            cart.CartIdentifier = cartIdentifier;
                            //add items
                            var cartItem = new CartItem
                            {
                                //CartId = cart.Id,
                                Name = cartItemDTO.Name,
                                Quantity = cartItemDTO.Quantity,
                                ProductId = cartItemDTO.ProductId,
                                Price = cartItemDTO.Price,
                                Cart = cart
                            };
                            _db.Carts.Add(cart);
                            _db.CartItems.Add(cartItem);
                            _db.SaveChanges();
                        }
                        else //user added to cart before
                        {
                            var cart = _db.Carts.Include(c=>c.CartItems).FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                            var cartItems = cart.CartItems.ToList();
                            foreach(var item in cartItems)
                            {
                                if(item.Name == cartItemDTO.Name)//item already exists
                                {
                                    item.Quantity++;
                                    _db.CartItems.Update(item);
                                    _db.SaveChanges();
                                    return Ok(new { message = "Item added to cart!" });
                                }
                            }
                            //add items
                            var cartItem = new CartItem
                            {
                                //CartId = cart.Id,
                                Name = cartItemDTO.Name,
                                Quantity = cartItemDTO.Quantity,
                                ProductId = cartItemDTO.ProductId,
                                Price = cartItemDTO.Price,
                                Cart = cart
                            };
                            _db.CartItems.Add(cartItem);
                            _db.SaveChanges();
                        }
                    }
                    else
                    {
                        var user_id = HttpContext.User.Claims.ToList()[1].Value;
                        var cart = _db.Carts.Include(c=>c.CartItems).FirstOrDefault(c => c.UserId == user_id);
                        if(cart == null)
                        {
                            cart = new Cart { UserId = user_id };
                            _db.Carts.Add(cart);
                            _db.SaveChanges();
                        }
                        var cartItems = cart.CartItems.ToList();
                        foreach( var item in cartItems)
                        {
                            if (item.Name == cartItemDTO.Name)
                            {
                                item.Quantity++;
                                _db.CartItems.Update(item);
                                _db.SaveChanges();
                                return Ok(new { message = "Item added to cart!" });
                            }
                        }
                        var cartItem = new CartItem
                        {
                            Name = cartItemDTO.Name,
                            Quantity = cartItemDTO.Quantity,
                            ProductId = cartItemDTO.ProductId,
                            Price = cartItemDTO.Price,
                            Cart = cart
                        };
                        _db.CartItems.Add(cartItem);
                        _db.SaveChanges();
                    }
                    return Ok(new { message = "Item added to cart!" });
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(e => e.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(errors);
                }
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPost("increase-item/{item_id:int}")]
        public async Task<IActionResult> IncreaseItem(int item_id)
        {
            try
            {
                if(item_id == null)
                {
                    return BadRequest("Enter item id.");
                }
                Cart cart = null;
                //find logged in user's cart or guest's cart
                if (!HttpContext.User.Identity.IsAuthenticated)//guest
                {
                    var cartIdentifier = Request.Cookies["CartIdentifier"];
                    if (cartIdentifier.IsNullOrEmpty())
                    {
                        return NotFound("Cart not found!");
                    }
                    else
                    {
                        cart = _db.Carts.FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                    }

                }
                else//logged in user
                {
                    string user_id = HttpContext.User.Claims.ToList()[1].Value;
                    cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user_id);
                    if (cart == null)
                    {
                        return NotFound("Cart not found!");
                    }
                }
                //find cart item to increase quantity
                var cartItem = _db.CartItems.FirstOrDefault(c => c.ProductId == item_id && c.CartId == cart.Id);
                if (cartItem == null)
                {
                    return NotFound(new { message = "item not found!" });
                }
                cartItem.Quantity++;
                _db.CartItems.Update(cartItem);
                _db.SaveChanges();
                return Ok(new { message = "item quantity was increased from the cart." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPost("decrease-item/{item_id:int}")]
        public async Task<IActionResult> DecreaseItem(int item_id)
        {
            try
            {
                if (item_id == null)
                {
                    return BadRequest("Enter item id.");
                }
                Cart cart = null;
                //find logged in user's cart or guest's cart
                if (!HttpContext.User.Identity.IsAuthenticated)//guest
                {
                    var cartIdentifier = Request.Cookies["CartIdentifier"];
                    if (cartIdentifier.IsNullOrEmpty())
                    {
                        return NotFound("Cart not found!");
                    }
                    else
                    {
                        cart = _db.Carts.FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                    }

                }
                else//logged in user
                {
                    string user_id = HttpContext.User.Claims.ToList()[1].Value;
                    cart = _db.Carts.FirstOrDefault(c => c.UserId == user_id);
                    if (cart == null)
                    {
                        return NotFound("Cart not found!");
                    }
                }
                //find cart item to decrease quantity
                var cartItem = _db.CartItems.FirstOrDefault(c => c.ProductId == item_id && c.CartId == cart.Id);
                if (cartItem == null)
                {
                    return NotFound(new { message = "item not found!" });
                }
                cartItem.Quantity--;
                if(cartItem.Quantity < 0)
                {
                    return BadRequest("Quantity must be more than 0.");
                }
                if(cartItem.Quantity == 0)
                {
                    _db.CartItems.Remove(cartItem);
                    _db.SaveChanges();
                    return Ok(new { message = "item was removed from the cart." });
                }
                _db.CartItems.Update(cartItem);
                _db.SaveChanges();
                return Ok(new { message = "item quantity was decreased from the cart." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPost("remove-item/{item_id:int}")]
        public async Task<IActionResult> RemoveItem(int item_id)
        {
            try
            {
                if (item_id == null)
                {
                    return BadRequest("Enter item id.");
                }
                Cart cart = null;
                //find logged in user's cart or guest's cart
                if (!HttpContext.User.Identity.IsAuthenticated)//guest
                {
                    var cartIdentifier = Request.Cookies["CartIdentifier"];
                    if (cartIdentifier.IsNullOrEmpty())
                    {
                        return NotFound("Cart not found!");
                    }
                    else
                    {
                        cart = _db.Carts.FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                    }

                }
                else//logged in user
                {
                    string user_id = HttpContext.User.Claims.ToList()[1].Value;
                    cart = _db.Carts.FirstOrDefault(c => c.UserId == user_id);
                    if (cart == null)
                    {
                        return NotFound("Cart not found!");
                    }
                }
                //find cart item to remove from cart
                var cartItem = _db.CartItems.FirstOrDefault(c => c.Id == item_id && c.CartId == cart.Id);
                if (cartItem == null)
                {
                    return NotFound(new { message = "item not found!" });
                }
                // remove from database
                _db.CartItems.Remove(cartItem);
                _db.SaveChanges();
                return Ok(new { message = "item was removed from the cart." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }
        [HttpPost("{id:int}/empty")]
        public async Task<IActionResult> EmptyCart(int id)
        {
            try
            {
                if (id == null)
                {
                    return BadRequest("Enter cart id.");
                }
                Cart cart = null;
                //find logged in user's cart or guest's cart
                if (!HttpContext.User.Identity.IsAuthenticated)//guest
                {
                    var cartIdentifier = Request.Cookies["CartIdentifier"];
                    if (cartIdentifier.IsNullOrEmpty())
                    {
                        return NotFound("Cart not found!");
                    }
                    else
                    {
                        cart = _db.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.CartIdentifier == cartIdentifier);
                    }

                }
                else//logged in user
                {
                    string user_id = HttpContext.User.Claims.ToList()[1].Value;
                    cart = _db.Carts.Include(c=>c.CartItems).FirstOrDefault(c => c.UserId == user_id);
                    if (cart == null)
                    {
                        return NotFound("Cart not found!");
                    }
                }
                //find cart items to remove from database
                var cartItems = cart.CartItems.ToList();
                if (cartItems.Count()==0)
                {
                    return BadRequest(new { message = "Cart is already empty!" });
                }
                else
                {
                    foreach (var item in cartItems)
                    {
                        _db.CartItems.Remove(item);
                    }
                }
                _db.SaveChanges();
                return Ok(new { message = "Cart has been empty successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                throw;
            }
        }

    }
}
