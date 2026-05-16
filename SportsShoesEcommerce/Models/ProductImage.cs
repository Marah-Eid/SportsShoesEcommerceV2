using System.ComponentModel.DataAnnotations;

namespace SportsShoesEcommerce.Models
{
    public class ProductImage
    {
        public int Id { get; set; }

        [Required]
        public string ImagePath { get; set; }

        public bool IsMain { get; set; } = false;

        public int ProductId { get; set; }
        public Product? Product { get; set; }

    }
}
