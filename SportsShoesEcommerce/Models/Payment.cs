using SportsShoesEcommerce.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace SportsShoesEcommerce.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public PaymentStatus PaymentStatus { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        public string? TransactionnId { get; set; }
    }
}
