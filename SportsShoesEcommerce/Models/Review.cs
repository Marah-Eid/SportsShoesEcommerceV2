using System.ComponentModel.DataAnnotations;

namespace SportsShoesEcommerce.Models
{
    public class Review
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public string? Comment { get; set; }
        public bool IsApproved { get; set; } = false;


        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
