using SportsShoesEcommerce.Models;
using System.Collections.Generic;

namespace SportsShoesEcommerce.Models.ViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
}