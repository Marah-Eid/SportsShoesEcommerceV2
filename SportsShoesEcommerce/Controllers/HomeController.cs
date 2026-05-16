using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Fetch Top 3 Categories for the Promo Cards
            var featuredCategories = await _context.Categories
                .Where(c => c.IsDeleted == false)
                .Take(3)
                .ToListAsync();

            // 2. Fetch Top 8 Discounted Products
            var featuredProducts = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.Discounts)
                .Where(p => p.IsDeleted == false &&
                            p.Discounts.Any(d => d.IsActive && d.StartDate <= DateTime.Now && d.EndDate >= DateTime.Now))
                .Take(8)
                .ToListAsync();

            // Fallback: If there are no active sales, just show the 8 newest products
            if (!featuredProducts.Any())
            {
                featuredProducts = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Discounts)
                    .Where(p => p.IsDeleted == false)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync();
            }

            // 3. Fetch Brands
            var brands = await _context.Brands
                .Where(b => b.IsDeleted == false)
                .ToListAsync();

            // 4. Fetch Approved Testimonials
            var testimonials = await _context.Testimonials
                .Include(t => t.User)
                .Where(t => t.IsApproved == true)
                .OrderByDescending(t => t.CreatedAt)
                .Take(4)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedCategories = featuredCategories,
                FeaturedProducts = featuredProducts,
                Brands = brands,
                Testimonials = testimonials
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}