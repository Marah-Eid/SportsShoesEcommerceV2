using System;

namespace SportsShoesEcommerce.Models
{
    public class Wishlist
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public ApplicationUser? User { get; set; }

        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }
}
