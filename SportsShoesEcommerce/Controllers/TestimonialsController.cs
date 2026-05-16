using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SportsShoesEcommerce.Data;
using SportsShoesEcommerce.Models;
using System;
using System.Threading.Tasks;

namespace SportsShoesEcommerce.Controllers
{
    // The [Authorize] tag means ONLY logged-in users can reach this page!
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

        // GET: Loads the Rate Us Page
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Saves the Review to the Database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int rating, string content)
        {
            if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Please provide a valid star rating and a written review.");
                return View();
            }

            // Find out who is currently logged in
            var user = await _userManager.GetUserAsync(User);

            var testimonial = new Testimonial
            {
                UserId = user.Id,
                Rating = rating,
                Content = content,
                IsApproved = false, // Sent to Admin for review!
                CreatedAt = DateTime.Now
            };

            _context.Testimonials.Add(testimonial);
            await _context.SaveChangesAsync();

            // Send a success message to the Home Page
            TempData["SuccessMessage"] = "Thank you for your review! It has been submitted and is pending admin approval.";
            return RedirectToAction("Index", "Home");
        }
    }
}
