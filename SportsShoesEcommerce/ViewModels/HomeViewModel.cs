using SportsShoesEcommerce.Models;
using System.Collections.Generic;

namespace SportsShoesEcommerce.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> FeaturedCategories { get; set; } = new List<Category>();
        public List<Product> FeaturedProducts { get; set; } = new List<Product>();
        public List<Brand> Brands { get; set; } = new List<Brand>();
        public List<Testimonial> Testimonials { get; set; } = new List<Testimonial>();
    }
}