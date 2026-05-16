using SportsShoesEcommerce.Models;
using System.Collections.Generic;

namespace SportsShoesEcommerce.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Review> Reviews { get; set; } = new List<Review>();

        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }

        public int NewRating { get; set; }
        public string NewReviewComment { get; set; }
    }
}
