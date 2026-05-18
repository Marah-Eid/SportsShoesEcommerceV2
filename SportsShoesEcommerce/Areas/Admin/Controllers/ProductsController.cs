using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index(string gender, int? brandId, int? categoryId, string stockStatus)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(gender) &&
                Enum.TryParse(typeof(Models.Enums.Gender), gender, out var genderEnum))
            {
                query = query.Where(p => p.Gender == (Models.Enums.Gender)genderEnum);
            }

            if (brandId.HasValue)
                query = query.Where(p => p.BrandId == brandId);

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            if (!string.IsNullOrEmpty(stockStatus))
            {
                if (stockStatus == "InStock")
                    query = query.Where(p => p.ProductVariants.Any(v => v.StockQuantity > 0));

                if (stockStatus == "OutOfStock")
                    query = query.Where(p => !p.ProductVariants.Any(v => v.StockQuantity > 0));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", brandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", categoryId);

            return View(await query.ToListAsync());
        }

        // =========================
        // CREATE GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // =========================
        // CREATE POST (FIXED)
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(

            Product product,
            string Size,
            string Color,
            int StockQuantity,
            string SKU,
            List<IFormFile> imageFiles)
        {

            Console.WriteLine(imageFiles?.Count ?? 0);
            ModelState.Remove("Brand");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
                return View(product);
            }

            // 1. Save product first
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // 2. Upload images
            if (imageFiles != null && imageFiles.Count > 0)
            {
                foreach (var file in imageFiles)
                {
                    if (file.Length <= 0) continue;

                    var imagePath = await SaveFile(file, "products");

                    _context.ProductImages.Add(new ProductImage
                    {
                        ProductId = product.Id,
                        ImagePath = imagePath   // IMPORTANT: FULL PATH
                    });
                }

                await _context.SaveChangesAsync();
            }

            // 3. Create default variant
            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                Size = string.IsNullOrEmpty(Size) ? "Default" : Size,
                Color = string.IsNullOrEmpty(Color) ? "Default" : Color,
                StockQuantity = StockQuantity,
                SKU = string.IsNullOrEmpty(SKU) ? $"SKU-{product.Id}" : SKU,
                VariantPrice = product.Price
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // EDIT GET
        // =========================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);

            return View(product);
        }

        // =========================
        // EDIT POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Product product,
            Dictionary<int, int> VariantQuantities,
            Dictionary<int, string> VariantSizes,
            Dictionary<int, string> VariantColors)
        {
            if (id != product.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            if (VariantQuantities != null)
            {
                foreach (var item in VariantQuantities)
                {
                    var variant = await _context.ProductVariants.FindAsync(item.Key);

                    if (variant != null && variant.ProductId == product.Id)
                    {
                        variant.StockQuantity = item.Value;

                        if (VariantSizes.ContainsKey(item.Key))
                            variant.Size = VariantSizes[item.Key];

                        if (VariantColors.ContainsKey(item.Key))
                            variant.Color = VariantColors[item.Key];

                        _context.ProductVariants.Update(variant);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // ADD VARIANT
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, string size, string color, int stockQuantity, decimal price)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            _context.ProductVariants.Add(new ProductVariant
            {
                ProductId = productId,
                Size = size,
                Color = color,
                StockQuantity = stockQuantity,
                SKU = $"{color}-{size}-{productId}",
                VariantPrice = price > 0 ? price : product.Price
            });

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // DELETE
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // FILE UPLOAD (FIXED + CLEAN)
        // =========================
        private async Task<string> SaveFile(IFormFile file, string folder)
        {
            var uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", folder);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // IMPORTANT: consistent DB format
            return $"/images/{folder}/{fileName}";
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}