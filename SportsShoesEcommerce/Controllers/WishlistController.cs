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


        [Authorize]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Redirect("/Identity/Account/Login");
            }

            var variant = await _context.ProductVariants.FirstOrDefaultAsync(v => v.ProductId == productId);

            if (variant == null)
            {
                variant = new ProductVariant
                {
                    ProductId = productId,
                    Size = "Standard",
                    Color = "Default"
                };
                _context.ProductVariants.Add(variant);
                await _context.SaveChangesAsync();
            }

            var existingItem = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.ProductVariantId == variant.Id && w.UserId == userId);

            if (existingItem != null)
            {
                TempData["ErrorMessage"] = "This shoe is already in your wishlist!";
            }
            else
            {
                var wishlistItem = new Wishlist
                {
                    ProductVariantId = variant.Id,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };

                _context.Wishlists.Add(wishlistItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Shoe added to your wishlist!";
            }

            string referer = Request.Headers["Referer"].ToString();
            return Redirect(string.IsNullOrEmpty(referer) ? "/" : referer);
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