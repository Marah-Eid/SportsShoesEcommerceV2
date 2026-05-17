using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class DiscountsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiscountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Discounts
        public async Task<IActionResult> Index(int? selectedDiscountId, bool? isActive)
        {
            var query = _context.Discounts.Include(d => d.Product).AsQueryable();

            if (selectedDiscountId.HasValue)
            {
                query = query.Where(d => d.Id == selectedDiscountId.Value);
            }

            if (isActive.HasValue)
            {
                var currentDate = DateTime.Now;
                if (isActive.Value)
                {
                    query = query.Where(d => d.StartDate <= currentDate && d.EndDate >= currentDate);
                }
                else
                {
                    query = query.Where(d => d.EndDate < currentDate);
                }
            }

            ViewBag.DiscountsDropdown = new SelectList(_context.Discounts.OrderBy(d => d.Title), "Id", "Title", selectedDiscountId);
            ViewData["CurrentSelectedDiscountId"] = selectedDiscountId;
            ViewData["CurrentStatusFilter"] = isActive;

            var discounts = await query.OrderByDescending(d => d.StartDate).ToListAsync();
            return View(discounts);
        }

        // GET: Admin/Discounts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (discount == null) return NotFound();

            return View(discount);
        }

        // GET: Admin/Discounts/Create
        public IActionResult Create()
        {
            ViewBag.ProductsDropdown = new SelectList(_context.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name), "Id", "Name");
            return View();
        }

        // POST: Admin/Discounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Discount discount)
        {
            if (ModelState.IsValid)
            {
                _context.Add(discount);
                await _context.SaveChangesAsync();

                if (discount.ProductId > 0)
                {
                    var product = await _context.Products.FindAsync(discount.ProductId);
                    if (product != null)
                    {
                        decimal discountAmount = product.Price * (discount.DiscountPercentage / 100m);
                        product.Price = product.Price - discountAmount;

                        _context.Update(product);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.ProductsDropdown = new SelectList(_context.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name), "Id", "Name", discount.ProductId);
            return View(discount);
        }

        // GET: Admin/Discounts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var discount = await _context.Discounts.FindAsync(id);
            if (discount == null) return NotFound();

            ViewBag.ProductsDropdown = new SelectList(_context.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name), "Id", "Name", discount.ProductId);
            return View(discount);
        }

        // POST: Admin/Discounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DiscountPercentage,StartDate,EndDate,IsActive,ProductId")] Discount discount)
        {
            if (id != discount.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oldDiscount = await _context.Discounts.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id);

                    if (oldDiscount != null && oldDiscount.ProductId > 0)
                    {
                        var product = await _context.Products.FindAsync(oldDiscount.ProductId);
                        if (product != null)
                        {
                            product.Price = product.Price / (1 - (oldDiscount.DiscountPercentage / 100m));

                            decimal newDiscountAmount = product.Price * (discount.DiscountPercentage / 100m);
                            product.Price = product.Price - newDiscountAmount;

                            _context.Update(product);
                        }
                    }

                    _context.Update(discount);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiscountExists(discount.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ProductsDropdown = new SelectList(_context.Products.Where(p => !p.IsDeleted).OrderBy(p => p.Name), "Id", "Name", discount.ProductId);
            return View(discount);
        }

        // GET: Admin/Discounts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var discount = await _context.Discounts
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (discount == null) return NotFound();

            return View(discount);
        }

        // POST: Admin/Discounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var discount = await _context.Discounts.FindAsync(id);
            if (discount != null)
            {
                if (discount.ProductId > 0)
                {
                    var product = await _context.Products.FindAsync(discount.ProductId);
                    if (product != null)
                    {
                        product.Price = product.Price / (1 - (discount.DiscountPercentage / 100m));
                        _context.Update(product);
                    }
                }

                _context.Discounts.Remove(discount);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DiscountExists(int id)
        {
            return _context.Discounts.Any(e => e.Id == id);
        }
    }
}