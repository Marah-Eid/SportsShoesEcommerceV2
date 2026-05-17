using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Orders
        public async Task<IActionResult> Index(string orderStatus, DateTime? startDate, DateTime? endDate)
        {
            // 1. بناء الاستعلام الأساسي مع الحفاظ على كل الـ Includes الأصلية والقوية الخاصة بكِ
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Address)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .AsQueryable();

            // 2. الفلترة المتقدمة حسب حالة الطلب (Enum)
            if (!string.IsNullOrEmpty(orderStatus))
            {
                if (Enum.TryParse(typeof(OrderStatus), orderStatus, out var statusEnum))
                {
                    query = query.Where(o => o.OrderStatus == (OrderStatus)statusEnum);
                }
            }

            // 3. الفلترة المتقدمة حسب تاريخ البداية (من تاريخ)
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            // 4. الفلترة المتقدمة حسب تاريخ النهاية (إلى تاريخ)
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value.AddDays(1));
            }

            // 5. الحفاظ على الترتيب التنازلي الأصلي لكِ لظهور أحدث الطلبات أولاً
            var orders = await query.OrderByDescending(o => o.Id).ToListAsync();

            // الاحتفاظ بالقيم المدخلة في الـ ViewData لتثبيتها داخل حقول الفلترة بالواجهة بعد البحث
            ViewData["CurrentStatus"] = orderStatus;
            ViewData["StartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate?.ToString("yyyy-MM-dd");

            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,AddressId,TotalPrice,OrderStatus,OrderDate")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,AddressId,TotalPrice,OrderStatus,OrderDate")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
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
            ViewData["AddressId"] = new SelectList(_context.Addresses, "Id", "City", order.AddressId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);


        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.Address)
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }



        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, SportsShoesEcommerce.Models.Enums.OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestOrder()
        {
            var defaultUser = await _context.Users.FirstOrDefaultAsync();
            if (defaultUser == null) return BadRequest("User table is empty.");

            var defaultAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.UserId == defaultUser.Id)
                                 ?? await _context.Addresses.FirstOrDefaultAsync();
            if (defaultAddress == null) return BadRequest("Address table is empty.");

            // جلب أول Variant متوفر للأحذية (مثل مقاس أو لون معين) لربطه بالطلب
            var defaultVariant = await _context.ProductVariants.FirstOrDefaultAsync();
            if (defaultVariant == null) return BadRequest("ProductVariants table is empty. Please add a variant first.");

            try
            {
                var testOrder = new Order
                {
                    UserId = defaultUser.Id,
                    AddressId = defaultAddress.Id,
                    TotalPrice = defaultVariant.Product.Price * 2, // سعر قطعتين من هذا الـ Variant
                    OrderStatus = SportsShoesEcommerce.Models.Enums.OrderStatus.Pending,
                    OrderDate = DateTime.Now
                };

                _context.Orders.Add(testOrder);
                await _context.SaveChangesAsync();

                // إضافة الـ Variant داخل تفاصيل عناصر الطلب
                var testItem = new OrderItem
                {
                    OrderId = testOrder.Id,
                    ProductVariantId = defaultVariant.Id,
                    Quantity = 2,
                    Price = defaultVariant.Product.Price
                };

                _context.OrderItems.Add(testItem);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems) // جلب العناصر المرتبطة أولاً لحذفها منعاً لمشاكل الـ Foreign Key
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order != null)
            {
                // 1. حذف كافة المنتجات والعناصر التابعة لهذا الطلب
                if (order.OrderItems != null && order.OrderItems.Any())
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                }

                // 2. حذف الطلب الأساسي نفسه
                _context.Orders.Remove(order);

                // 3. حفظ التغييرات نهائياً في قاعدة البيانات
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
