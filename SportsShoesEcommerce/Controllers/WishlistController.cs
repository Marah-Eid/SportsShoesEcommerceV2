using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System.Security.Claims;

namespace SportsShoesEcommerce.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();

            var wishlistItems = await _context.Wishlists
                .Include(w => w.ProductVariant)
                .ThenInclude(v => v.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(w => w.UserId == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productVariantId)
        {
            var userId = GetUserId();

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.UserId == userId &&
                    w.ProductVariantId == productVariantId);

            if (existingItem != null)
            {
                TempData["Info"] = "Product already exists in wishlist.";

                return RedirectToAction("Index", "Products");
            }

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                ProductVariantId = productVariantId
            };

            _context.Wishlists.Add(wishlistItem);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Product added to wishlist.";

            return RedirectToAction("Index", "Products");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var userId = GetUserId();

            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w =>
                    w.Id == id &&
                    w.UserId == userId);

            if (item == null)
            {
                TempData["Error"] = "Wishlist item not found.";

                return RedirectToAction("Index");
            }

            _context.Wishlists.Remove(item);

            await _context.SaveChangesAsync();

            TempData["Info"] = "Product removed from wishlist.";

            return RedirectToAction("Index");
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}