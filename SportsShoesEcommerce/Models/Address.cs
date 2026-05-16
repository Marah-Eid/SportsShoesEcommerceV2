using System.ComponentModel.DataAnnotations;

namespace SportsShoesEcommerce.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        public string? PostalCode { get; set; }

        public ICollection<Order>? Orders { get; set; }
    }
}
