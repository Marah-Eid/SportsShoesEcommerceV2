using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsShoesEcommerce.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        [Required]
        public string Size { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public string SKU { get; set; }

        public int StockQuantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal VariantPrice { get; set; }

        public ICollection<CartItem>? CartItems { get; set; }
        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Wishlist>? Wishlists { get; set; }

    }
}
