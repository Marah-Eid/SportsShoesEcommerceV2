using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.ViewModels;

namespace SportsShoesEcommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Include(p => p.Discounts)
                .Include(p => p.Reviews.Where(r => r.IsApproved == true))
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsDeleted == false);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Reviews = product.Reviews.OrderByDescending(r => r.CreatedAt).ToList(),
                TotalReviews = product.Reviews.Count,
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0
            };

            return View(viewModel);
        }

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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int ProductId, int NewRating, string NewReviewComment)
        {
            if (NewRating < 1 || NewRating > 5 || string.IsNullOrWhiteSpace(NewReviewComment))
            {
                TempData["ErrorMessage"] = "Invalid review data. Please provide a rating and a comment.";
                return RedirectToAction(nameof(Details), new { id = ProductId });
            }

            var user = await _userManager.GetUserAsync(User);

            var review = new Review
            {
                ProductId = ProductId,
                UserId = user.Id,
                Rating = NewRating,
                Comment = NewReviewComment,
                IsApproved = false,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you! Your review is pending moderation.";
            return RedirectToAction(nameof(Details), new { id = ProductId });
        }
    }
}