using System.ComponentModel.DataAnnotations;

namespace SportsShoesEcommerce.ViewModels
{
    public class CheckoutViewModel
    {
        [Required]
        public string City { get; set; } = string.Empty;

        [Required]
        public string Street { get; set; } = string.Empty;

        public string? PostalCode { get; set; }

        public CartViewModel Cart { get; set; } = new();
    }
}