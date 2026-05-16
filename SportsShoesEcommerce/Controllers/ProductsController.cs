using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.ViewModels; // Added for the new ViewModel
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Added to get the logged-in user

        // Updated constructor to include UserManager
        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // YOUR ORIGINAL INDEX METHOD (Untouched)
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsDeleted == false)
                .ToListAsync();

            return View(products);
        }

        // THE UPGRADED DETAILS METHOD
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Include(p => p.Discounts) // We need discounts to calculate sale prices!
                .Include(p => p.Reviews.Where(r => r.IsApproved == true)) // Your smart filter!
                    .ThenInclude(r => r.User) // We need the User to show their Profile Pic/Name
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null)
            {
                return NotFound();
            }

            // Package it all into the ViewModel
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
                TotalReviews = product.Reviews.Count,
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0
            };

            return View(viewModel);
        }

        // YOUR ORIGINAL SEARCH METHOD (Untouched)
        public async Task<IActionResult> Search(string searchText)
        {
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Where(p => p.IsDeleted == false);

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                products = products.Where(p =>
                    p.Name.Contains(searchText) ||
                    p.Category.Name.Contains(searchText));
            }

            ViewBag.SearchText = searchText;

            return View("Index", await products.ToListAsync());
        }

        // THE NEW ADD REVIEW METHOD
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int ProductId, int NewRating, string NewReviewComment)
        {
            // 1. Validate the input
            if (NewRating < 1 || NewRating > 5 || string.IsNullOrWhiteSpace(NewReviewComment))
            {
                TempData["ErrorMessage"] = "Invalid review data. Please provide a rating and a comment.";
                return RedirectToAction(nameof(Details), new { id = ProductId });
            }

            // 2. Get the logged in user
            var user = await _userManager.GetUserAsync(User);

            // 3. Create the review
            var review = new Review
            {
                ProductId = ProductId,
                UserId = user.Id,
                Rating = NewRating,
                Comment = NewReviewComment,
                IsApproved = false, // Must be approved by Admin
                CreatedAt = DateTime.Now
            };

            // 4. Save to DB
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // 5. Send success message back to the page
            TempData["SuccessMessage"] = "Thank you! Your review is pending moderation.";
            return RedirectToAction(nameof(Details), new { id = ProductId });
        }
    }
}