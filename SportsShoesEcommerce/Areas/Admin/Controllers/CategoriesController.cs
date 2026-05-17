using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // ركز هنا: يجب أن يكون اسم المتغير imageFile مطابقاً لاسم الـ input في الـ View
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] Category category, IFormFile imageFile)
        {
            // 1. تحقق أن الملف وصل فعلاً
            if (imageFile != null && imageFile.Length > 0)
            {
                // تجهيز المجلد والمسار
                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");
                if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string fullPath = Path.Combine(folderPath, fileName);

                // حفظ الصورة في المجلد
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // إسناد اسم الصورة للـ Model قبل الحفظ
                category.ImagePath = fileName;
            }
            else
            {
                // إذا لم تصل الصورة، سنعيد المستخدم للصفحة مع رسالة خطأ
                ModelState.AddModelError("imageFile", "Please upload an image.");
                return View(category);
            }

            // 2. الحفظ في قاعدة البيانات
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. أضفنا IFormFile imageFile لاستقبال الصورة الجديدة
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImagePath,IsDeleted")] Category category, IFormFile? imageFile)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // 2. تحديد المسار لمجلد categories
                        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories");

                        // التأكد من وجود المجلد
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // 3. توليد اسم جديد وحفظ الملف
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        // 4. تعيين اسم الصورة الجديد في الموديل
                        category.ImagePath = fileName;
                    }
                    else
                    {
                        // 5. حركة ذكية: إذا لم يرفع صورة، نمنع الـ Entity Framework من تعديل حقل الصورة
                        // لضمان بقاء الصورة القديمة كما هي في قاعدة البيانات
                        _context.Entry(category).Property(x => x.ImagePath).IsModified = false;
                    }

                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
