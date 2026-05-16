using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.ViewModels;
using System.Security.Claims;
using System.Text.Json;

namespace SportsShoesEcommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                await MergeSessionCartIntoDatabase();
            }

            var cart = await GetCartViewModel();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productVariantId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == productVariantId);

            if (variant == null)
            {
                TempData["Error"] = "Product variant not found.";
                return RedirectToAction("Index", "Products");
            }

            if (variant.StockQuantity <= 0)
            {
                TempData["Error"] = "This product is out of stock.";
                return RedirectToAction("Index");
            }

            bool added;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                added = await AddToDatabaseCart(productVariantId, variant.StockQuantity);
            }
            else
            {
                added = AddToSessionCart(productVariantId, variant.StockQuantity);
            }

            if (added)
            {
                TempData["Success"] = "Product added to cart successfully.";
            }
            else
            {
                TempData["Error"] = "You cannot add more than available stock.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Increase(int productVariantId)
        {
            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.Id == productVariantId);

            if (variant == null)
            {
                TempData["Error"] = "Product variant not found.";
                return RedirectToAction("Index");
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();

                var item = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci =>
                        ci.ProductVariantId == productVariantId &&
                        ci.Cart.UserId == userId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                if (item.Quantity >= variant.StockQuantity)
                {
                    TempData["Error"] = "You cannot add more than available stock.";
                    return RedirectToAction("Index");
                }

                item.Quantity++;
                await _context.SaveChangesAsync();

                TempData["Success"] = "Quantity increased.";
            }
            else
            {
                var cart = GetSessionCart();

                var item = cart.FirstOrDefault(x => x.ProductVariantId == productVariantId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                if (item.Quantity >= variant.StockQuantity)
                {
                    TempData["Error"] = "You cannot add more than available stock.";
                    return RedirectToAction("Index");
                }

                item.Quantity++;
                SaveSessionCart(cart);

                TempData["Success"] = "Quantity increased.";
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Decrease(int productVariantId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();

                var item = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci =>
                        ci.ProductVariantId == productVariantId &&
                        ci.Cart.UserId == userId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    _context.CartItems.Remove(item);

                    TempData["Info"] = "Product removed from cart.";
                }
                else
                {
                    TempData["Success"] = "Quantity updated.";
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var cart = GetSessionCart();

                var item = cart.FirstOrDefault(x => x.ProductVariantId == productVariantId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);

                    TempData["Info"] = "Product removed from cart.";
                }
                else
                {
                    TempData["Success"] = "Quantity updated.";
                }

                SaveSessionCart(cart);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int productVariantId)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();

                var item = await _context.CartItems
                    .Include(ci => ci.Cart)
                    .FirstOrDefaultAsync(ci =>
                        ci.ProductVariantId == productVariantId &&
                        ci.Cart.UserId == userId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();

                TempData["Info"] = "Product removed from cart.";
            }
            else
            {
                var cart = GetSessionCart();

                var item = cart.FirstOrDefault(x => x.ProductVariantId == productVariantId);

                if (item == null)
                {
                    TempData["Error"] = "Item not found in cart.";
                    return RedirectToAction("Index");
                }

                cart.Remove(item);
                SaveSessionCart(cart);

                TempData["Info"] = "Product removed from cart.";
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Clear()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = GetUserId();

                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                {
                    TempData["Info"] = "Your cart is already empty.";
                    return RedirectToAction("Index");
                }

                _context.CartItems.RemoveRange(cart.CartItems);
                await _context.SaveChangesAsync();

                TempData["Info"] = "Cart cleared successfully.";
            }
            else
            {
                var cart = GetSessionCart();

                if (cart.Count == 0)
                {
                    TempData["Info"] = "Your cart is already empty.";
                    return RedirectToAction("Index");
                }

                HttpContext.Session.Remove(CartSessionKey);

                TempData["Info"] = "Cart cleared successfully.";
            }

            return RedirectToAction("Index");
        }

        private async Task MergeSessionCartIntoDatabase()
        {
            var sessionCart = GetSessionCart();

            if (sessionCart.Count == 0)
            {
                return;
            }

            var mergedAnyItem = false;

            foreach (var item in sessionCart)
            {
                var variant = await _context.ProductVariants
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);

                if (variant != null && variant.StockQuantity > 0)
                {
                    var added = await AddToDatabaseCart(
                        item.ProductVariantId,
                        variant.StockQuantity,
                        item.Quantity
                    );

                    if (added)
                    {
                        mergedAnyItem = true;
                    }
                }
            }

            HttpContext.Session.Remove(CartSessionKey);

            if (mergedAnyItem)
            {
                TempData["Success"] = "Your cart was restored after login.";
            }
            else
            {
                TempData["Info"] = "Your previous cart items are no longer available.";
            }
        }

        private async Task<bool> AddToDatabaseCart(int productVariantId, int stockQuantity, int quantity = 1)
        {
            var userId = GetUserId();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };

                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci =>
                    ci.CartId == cart.Id &&
                    ci.ProductVariantId == productVariantId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > stockQuantity)
                {
                    return false;
                }

                existingItem.Quantity += quantity;
            }
            else
            {
                if (quantity > stockQuantity)
                {
                    return false;
                }

                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                };

                _context.CartItems.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private bool AddToSessionCart(int productVariantId, int stockQuantity, int quantity = 1)
        {
            var cart = GetSessionCart();

            var existingItem = cart.FirstOrDefault(x => x.ProductVariantId == productVariantId);

            if (existingItem != null)
            {
                if (existingItem.Quantity + quantity > stockQuantity)
                {
                    return false;
                }

                existingItem.Quantity += quantity;
            }
            else
            {
                if (quantity > stockQuantity)
                {
                    return false;
                }

                cart.Add(new CartSessionItem
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                });
            }

            SaveSessionCart(cart);
            return true;
        }
        private async Task<CartViewModel> GetCartViewModel()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return await GetDatabaseCartViewModel();
            }

            return await GetSessionCartViewModel();
        }

        private async Task<CartViewModel> GetDatabaseCartViewModel()
        {
            var userId = GetUserId();

            var cartViewModel = new CartViewModel();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.ProductVariant)
                        .ThenInclude(v => v.Product)
                            .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || cart.CartItems == null)
            {
                return cartViewModel;
            }

            foreach (var item in cart.CartItems)
            {
                var variant = item.ProductVariant;

                if (variant != null)
                {
                    var image = variant.Product?.ProductImages?
                        .FirstOrDefault(x => x.IsMain)?.ImagePath
                        ?? variant.Product?.ProductImages?.FirstOrDefault()?.ImagePath;

                    cartViewModel.Items.Add(new CartItemViewModel
                    {
                        CartItemId = item.Id,
                        ProductVariantId = variant.Id,
                        ProductName = variant.Product?.Name ?? "",
                        ImagePath = image,
                        Size = variant.Size,
                        Color = variant.Color,
                        Price = variant.VariantPrice,
                        Quantity = item.Quantity
                    });
                }
            }

            return cartViewModel;
        }

        private async Task<CartViewModel> GetSessionCartViewModel()
        {
            var sessionCart = GetSessionCart();
            var cartViewModel = new CartViewModel();

            foreach (var item in sessionCart)
            {
                var variant = await _context.ProductVariants
                    .Include(v => v.Product)
                    .ThenInclude(p => p.ProductImages)
                    .FirstOrDefaultAsync(v => v.Id == item.ProductVariantId);

                if (variant != null)
                {
                    var image = variant.Product?.ProductImages?
                        .FirstOrDefault(x => x.IsMain)?.ImagePath
                        ?? variant.Product?.ProductImages?.FirstOrDefault()?.ImagePath;

                    cartViewModel.Items.Add(new CartItemViewModel
                    {
                        ProductVariantId = variant.Id,
                        ProductName = variant.Product?.Name ?? "",
                        ImagePath = image,
                        Size = variant.Size,
                        Color = variant.Color,
                        Price = variant.VariantPrice,
                        Quantity = item.Quantity
                    });
                }
            }

            return cartViewModel;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private List<CartSessionItem> GetSessionCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartSessionItem>();
            }

            return JsonSerializer.Deserialize<List<CartSessionItem>>(cartJson) ?? new List<CartSessionItem>();
        }

        private void SaveSessionCart(List<CartSessionItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, cartJson);
        }
    }

    public class CartSessionItem
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}