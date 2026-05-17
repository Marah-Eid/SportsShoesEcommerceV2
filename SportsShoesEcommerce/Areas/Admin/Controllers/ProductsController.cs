using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = _context.Products
          .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages);

            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name");
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Price,Gender,CategoryId")] Product product, string brandName, IFormFile? imageFile, IFormFile? brandLogoFile)
        {
            ModelState.Remove("Brand");
            ModelState.Remove("Category");
            ModelState.Remove("imageFile");
            ModelState.Remove("brandLogoFile");
            ModelState.Remove("brandName");

            if (ModelState.IsValid)
            {
                // --- 1. معالجة البراند (موجود أو جديد) ---
                var existingBrand = await _context.Brands
                    .FirstOrDefaultAsync(b => b.Name.ToLower() == brandName.Trim().ToLower());

                Brand brandToUpdate;

                if (existingBrand != null)
                {
                    brandToUpdate = existingBrand;
                    product.BrandId = existingBrand.Id;
                }
                else
                {
                    brandToUpdate = new Brand
                    {
                        Name = brandName.Trim(),
                        Logo = "default-logo.png",
                        IsDeleted = false
                    };
                    _context.Brands.Add(brandToUpdate);
                    await _context.SaveChangesAsync();
                    product.BrandId = brandToUpdate.Id;
                }

                // --- 2. حفظ / تحديث لوجو البراند (إذا تم رفع ملف) ---
                if (brandLogoFile != null && brandLogoFile.Length > 0)
                {
                    string brandFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "brands");
                    if (!Directory.Exists(brandFolderPath)) Directory.CreateDirectory(brandFolderPath);

                    string brandLogoName = Guid.NewGuid().ToString() + Path.GetExtension(brandLogoFile.FileName);
                    string brandFilePath = Path.Combine(brandFolderPath, brandLogoName);

                    using (var stream = new FileStream(brandFilePath, FileMode.Create))
                    {
                        await brandLogoFile.CopyToAsync(stream);
                    }

                    brandToUpdate.Logo = brandLogoName;
                    _context.Update(brandToUpdate);
                    await _context.SaveChangesAsync();
                }

                // --- 3. حفظ المنتج الأساسي ---
                product.CreatedAt = DateTime.Now;
                _context.Add(product);
                await _context.SaveChangesAsync();

                // --- 4. حفظ صورة المنتج ---
                if (imageFile != null && imageFile.Length > 0)
                {
                    string productFolderPath = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(productFolderPath)) Directory.CreateDirectory(productFolderPath);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(productFolderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    var newProductImage = new ProductImage
                    {
                        ImagePath = fileName,
                        ProductId = product.Id,
                        IsMain = true
                    };
                    _context.ProductImages.Add(newProductImage);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
            {
            if (id == null) return NotFound();

            var product = await _context.Products.Include(p => p.ProductImages).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
            }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Price,Gender,CreatedAt,IsDeleted,BrandId,CategoryId")] Product product)
        {
            if (id != product.Id) return NotFound();

            ModelState.Remove("Brand");
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["BrandId"] = new SelectList(_context.Brands, "Id", "Name", product.BrandId);
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
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

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
