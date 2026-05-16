using Microsoft.AspNetCore.Mvc;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System.Security.Claims;

namespace SportsShoesEcommerce.Controllers
{
    public class ContactController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(ContactMessage message)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", message);
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                message.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            }

            message.CreatedAt = DateTime.Now;
            message.IsRead = false;

            _context.ContactMessages.Add(message);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Your message has been sent successfully.";

            return RedirectToAction("Index");
        }
    }
}