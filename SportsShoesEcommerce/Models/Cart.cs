namespace SportsShoesEcommerce.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<CartItem>? CartItems { get; set; }

    }
}
