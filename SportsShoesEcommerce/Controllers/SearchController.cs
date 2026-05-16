using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Controllers
{
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string q)
        {
            var viewModel = new SearchViewModel { Query = q ?? string.Empty };

            if (!string.IsNullOrWhiteSpace(q))
            {
                // We removed .ToLower() because SQL Server is already case-insensitive.
                // We added strict null checks (p.Category != null) to prevent database crashes!

                viewModel.Products = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages)
                    .Include(p => p.ProductVariants)
                    .Include(p => p.Discounts)
                    .Where(p => p.IsDeleted == false &&
                                (
                                    (p.Name != null && p.Name.Contains(q)) ||
                                    (p.Category != null && p.Category.Name != null && p.Category.Name.Contains(q)) ||
                                    (p.Brand != null && p.Brand.Name != null && p.Brand.Name.Contains(q))
                                ))
                    .ToListAsync();
            }

            return View(viewModel);
        }
    }
}