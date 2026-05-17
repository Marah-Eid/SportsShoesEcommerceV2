using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Index(string gender, int? brandId, int? categoryId, string stockStatus)
        {
            var query = _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductVariants)
                .Include(p => p.Discounts)
                .Where(p => !p.IsDeleted);

            if (!string.IsNullOrEmpty(gender))
            {
                if (Enum.TryParse(typeof(SportsShoesEcommerce.Models.Enums.Gender), gender, out var genderEnum))
                {
                    query = query.Where(p => p.Gender == (SportsShoesEcommerce.Models.Enums.Gender)genderEnum);
                }
            }

            if (brandId.HasValue)
            {
                query = query.Where(p => p.BrandId == brandId);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                if (stockStatus == "InStock")
                {
                    query = query.Where(p => p.ProductVariants.Any(pv => pv.StockQuantity > 0));
                }
                else if (stockStatus == "OutOfStock")
                {
                    query = query.Where(p => !p.ProductVariants.Any(pv => pv.StockQuantity > 0));
                }
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", brandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", categoryId);

            ViewData["CurrentGender"] = gender;
            ViewData["CurrentStockStatus"] = stockStatus;

            return View(await query.ToListAsync());
        }

        // GET: Admin/Products/Create
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands.OrderBy(b => b.Name), "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
            return View();
        }

        // POST: Admin/Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, string Size, string Color, int StockQuantity, string SKU, List<IFormFile> imageFiles)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();

                if (imageFiles != null && imageFiles.Any())
                {
                    foreach (var file in imageFiles)
                    {
                        if (file.Length > 0)
                        {
                            string savedFileName = await UploadFile(file, "products");
                            var productImage = new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = savedFileName
                            };
                            _context.ProductImages.Add(productImage);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                var initialVariant = new ProductVariant
                {
                    ProductId = product.Id,
                    Size = !string.IsNullOrEmpty(Size) ? Size : "Default",
                    Color = !string.IsNullOrEmpty(Color) ? Color : "Default",
                    StockQuantity = StockQuantity,
                    SKU = !string.IsNullOrEmpty(SKU) ? SKU : $"SKU-{product.Id}",
                    VariantPrice = product.Price
                };

                _context.ProductVariants.Add(initialVariant);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands.OrderBy(b => b.Name), "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 1. دالة فتح صفحة التعديل وجلب البيانات الحالية (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // جلب المنتج وتضمين المقاسات (Variants) التابعة له لتعديلها
            var product = await _context.Products
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, Dictionary<int, int> VariantQuantities, Dictionary<int, string> VariantSizes, Dictionary<int, string> VariantColors)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. تحديث البيانات الأساسية للمنتج (الاسم، السعر، إلخ)
                    _context.Update(product);
                    await _context.SaveChangesAsync();

                    // 2. تحديث كمية ومقاس ولون كل Variant تم تعديله في الصفحة
                    if (VariantQuantities != null)
                    {
                        foreach (var item in VariantQuantities)
                        {
                            var variantId = item.Key;
                            var newQuantity = item.Value;

                            var variant = await _context.ProductVariants.FindAsync(variantId);
                            if (variant != null && variant.ProductId == product.Id)
                            {
                                variant.StockQuantity = newQuantity; // تعديل الكمية لكل نوع عنجد
                                if (VariantSizes.ContainsKey(variantId)) variant.Size = VariantSizes[variantId];
                                if (VariantColors.ContainsKey(variantId)) variant.Color = VariantColors[variantId];

                                _context.ProductVariants.Update(variant);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.Id == product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(int productId, string size, string color, int stockQuantity, decimal price)
        {
            // إنشاء توليفة المقاس واللون والكمية الجديدة وضخها بالـ Database
            var newVariant = new ProductVariant
            {
                ProductId = productId,
                Size = size,
                Color = color,
                StockQuantity = stockQuantity, // الكمية الخاصة بهذا النوع بالتحديد
                SKU = $"{color.Substring(0, Math.Min(3, color.Length)).ToUpper()}-{size}-{productId}",
                VariantPrice = price > 0 ? price : _context.Products.Find(productId)?.Price ?? 0
            };

            _context.ProductVariants.Add(newVariant);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // 8. تأكيد الحذف (POST) - كانت ناقصة
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

        private async Task<string> UploadFile(IFormFile file, string folder)
        {
            string folderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", folder);
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return fileName;
        }
        [HttpPost]
        public async Task<IActionResult> UpdateStock(int variantId, int quantity)
        {
            var variant = await _context.ProductVariants.FindAsync(variantId);
            if (variant != null)
            {
                variant.StockQuantity = quantity; // وضع الكمية الحقيقية التي حددتِها بالمخزن
                _context.ProductVariants.Update(variant);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}