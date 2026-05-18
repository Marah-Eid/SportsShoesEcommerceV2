using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;

namespace SportsShoesEcommerce.Controllers
{
    [Authorize]
    public class TestimonialsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestimonialsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int rating, string content)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Please provide a valid star rating and a written review.");
                return View();
            }

            var user = await _userManager.GetUserAsync(User);

            var testimonial = new Testimonial
            {
                UserId = user.Id,
                Rating = rating,
                Content = content,
                IsApproved = false,
                CreatedAt = DateTime.Now
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thank you for your review! It has been submitted and is pending admin approval.";
            return RedirectToAction("Index", "Home");
        }
    }
}
