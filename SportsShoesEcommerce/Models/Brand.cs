namespace SportsShoesEcommerce.Models
{
    public class Brand
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<Product>? Products { get; set; }

    }
}
