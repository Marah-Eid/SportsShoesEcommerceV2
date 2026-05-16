namespace SportsShoesEcommerce.ViewModels
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public decimal TotalPrice
        {
            get
            {
                return Items.Sum(i => i.Total);
            }
        }
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }
        public int ProductVariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }

        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;

        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total
        {
            get
            {
                return Price * Quantity;
            }
        }
    }
}