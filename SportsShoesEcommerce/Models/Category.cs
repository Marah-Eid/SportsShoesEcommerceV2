using System.Collections.Generic;

namespace SportsShoesEcommerce.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string ImagePath { get; set; }

        public bool IsDeleted { get; set; } = false;

        public ICollection<Product>? Products { get; set; }
    }
}
