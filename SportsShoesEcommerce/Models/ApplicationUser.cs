using Microsoft.AspNetCore.Identity;

namespace SportsShoesEcommerce.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsBlocked { get; set; } = false;

        public string? ProfileImage { get; set; }
        public ICollection<Testimonial>? Testimonials { get; set; }

    }
}
