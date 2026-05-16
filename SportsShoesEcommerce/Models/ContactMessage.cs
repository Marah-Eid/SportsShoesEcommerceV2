using System.ComponentModel.DataAnnotations;

namespace SportsShoesEcommerce.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        public string? PhoneNumber { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;

        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

    }
}
