using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.Enums;

namespace SportsShoesEcommerce.ViewModels
{
    public class ProductFilterViewModel
    {
        public string? SearchText { get; set; }

        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public Gender? Gender { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; }

        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Brand> Brands { get; set; } = new();
    }
}