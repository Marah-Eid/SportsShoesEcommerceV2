using Microsoft.AspNetCore.Mvc;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;

namespace SportsShoesEcommerce.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ContactMessage message)
        {
            if (ModelState.IsValid)
            {
                message.CreatedAt = DateTime.Now;
                _context.ContactMessages.Add(message);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Thank you for reaching out! Our team will get back to you shortly.";
                return RedirectToAction(nameof(Index));
            }

            return View("Index", message);
        }
    }
}