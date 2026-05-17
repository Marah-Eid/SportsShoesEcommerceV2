using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestimonialsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. تعديل الـ Index لترتيب النتائج (غير الموافق عليه أولاً)
        public async Task<IActionResult> Index()
        {
            var testimonials = _context.Testimonials
                .Include(t => t.User)
                .OrderBy(t => t.IsApproved); // False تظهر قبل True

            return View(await testimonials.ToListAsync());
        }

        // 2. إضافة دالة الموافقة (Approve) - أهم تعديل
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial == null)
            {
                return NotFound();
            }

            testimonial.IsApproved = true;
            _context.Update(testimonial);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // 3. بقية الدوال (Details, Delete) تبقى كما هي للحاجة
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var testimonial = await _context.Testimonials
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (testimonial == null) return NotFound();

            return View(testimonial);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var testimonial = await _context.Testimonials.FindAsync(id);
            if (testimonial != null)
            {
                _context.Testimonials.Remove(testimonial);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TestimonialExists(int id)
        {
            return _context.Testimonials.Any(e => e.Id == id);
        }
    }
}