using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System.Diagnostics;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.TotalProducts = await _context.Products.CountAsync();
            ViewBag.TotalCategories = await _context.Categories.CountAsync();
            ViewBag.TotalOrders = await _context.Orders.CountAsync();

            ViewBag.PendingReviews = await _context.Reviews.CountAsync(r => !r.IsApproved);
            ViewBag.PendingTestimonials = await _context.Testimonials.CountAsync(t => !t.IsApproved);

            var salesData = await _context.Orders
                .Where(o => o.OrderDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Month + "/" + g.Key.Year,
                    Total = g.Sum(o => o.TotalPrice),
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.SalesLabels = salesData.Select(s => s.Month).ToList();
            ViewBag.SalesValues = salesData.Select(s => s.Total).ToList();


            var categoryData = await _context.Products
                .Where(p => !p.IsDeleted)
                .GroupBy(p => p.Category.Name)
                .Select(g => new
                {
                    CategoryName = g.Key ?? "Uncategorized",
                    ProductCount = g.Count()
                })
                .ToListAsync();

            ViewBag.CategoryLabels = categoryData.Select(c => c.CategoryName).ToList();
            ViewBag.CategoryValues = categoryData.Select(c => c.ProductCount).ToList();


            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            return View(recentOrders);
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