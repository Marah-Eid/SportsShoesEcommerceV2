using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SportsShoesEcommerce.Models;
using SportsShoesEcommerce.Models.Enums;

namespace SportsShoesEcommerce.Data.Seeders
{
    public static class DbSeeder
    {
        // ============================================================
        // Main entry point — call this from Program.cs
        // ============================================================
        public static async Task SeedAllAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedAdminUserAsync(userManager);
            await SeedBrandsAsync(context);
            await SeedCategoriesAsync(context);
            await SeedProductsAsync(context);
            await SeedProductVariantsAsync(context);
            await SeedProductImagesAsync(context);
            await SeedDiscountsAsync(context);
        }

        // ============================================================
        // IMAGE LIBRARY
        // - Brand logos: Google's S2 favicon service (works for any
        //   domain, no API key, returns the brand's actual icon).
        // - Category & product photos: Verified Unsplash CDN URLs.
        //   Kept the pool small and curated so every shot reads as
        //   "athletic sneaker" — no random mismatched gear.
        // ============================================================
        private static class Images
        {
            // Unsplash URL builder (cool, slightly desaturated look
            // to match the silver/slate site palette).
            private static string U(string photoId, int w = 800) =>
                $"https://images.unsplash.com/photo-{photoId}?w={w}&q=80&auto=format&fit=crop";

            // Brand logo via Google favicon CDN — free, no key required.
            // The sz parameter requests a 128x128 PNG.
            private static string Logo(string domain) =>
                $"https://www.google.com/s2/favicons?sz=128&domain={domain}";

            public static class Logos
            {
                public static string Nike = Logo("nike.com");
                public static string Adidas = Logo("adidas.com");
                public static string Puma = Logo("puma.com");
                public static string NewBalance = Logo("newbalance.com");
                public static string Reebok = Logo("reebok.com");
                public static string Asics = Logo("asics.com");
                public static string UnderArmour = Logo("underarmour.com");
                public static string Converse = Logo("converse.com");
            }

            // Category hero images — chosen to fit the cool silver/slate
            // aesthetic (mostly neutral, studio-style sneaker shots).
            public static class Categories
            {
                public static string Running = U("1542291026-7eec264c27ff");  // classic running sneaker
                public static string Training = U("1571019613454-1cb2f99b2d8b");  // studio gym trainer
                public static string Basketball = U("1552346154-21d32810aba3");  // sneaker on white bg
                public static string Soccer = U("1511886929837-354d827aae26");  // cleats studio shot
                public static string CourtSports = U("1595950653106-6c9ebd614d3a");  // white tennis-style shoe
                public static string Outdoor = U("1520975954732-35dd22299614");  // trail shoes on rocks
            }

            // ---- Small, curated product photo pools ----
            // I cut the pool down to 3-4 verified photos per category.
            // Each photo is a clean, studio-style shoe shot — no random
            // gym equipment, watches, or unrelated gear.
            public static readonly string[] RunningPool = {
                U("1542291026-7eec264c27ff"),  // red runners studio
                U("1595950653106-6c9ebd614d3a"),  // white runner clean
                U("1539185441755-769473a23570"),  // grey runners
                U("1606107557195-0e29a4b5b4aa"),  // sneaker side profile
            };

            public static readonly string[] TrainingPool = {
                U("1571019613454-1cb2f99b2d8b"),  // grey trainer
                U("1606107557195-0e29a4b5b4aa"),  // clean side shot
                U("1542291026-7eec264c27ff"),  // athletic shoe
                U("1595950653106-6c9ebd614d3a"),  // training shoe white bg
            };

            public static readonly string[] BasketballPool = {
                U("1552346154-21d32810aba3"),  // basketball sneaker
                U("1606107557195-0e29a4b5b4aa"),  // high-top side
                U("1542291026-7eec264c27ff"),  // red basketball-like
                U("1595950653106-6c9ebd614d3a"),  // white hi-top
            };

            public static readonly string[] SoccerPool = {
                U("1511886929837-354d827aae26"),  // soccer cleats studio
                U("1595950653106-6c9ebd614d3a"),  // clean athletic
                U("1542291026-7eec264c27ff"),  // sport shoe
                U("1606107557195-0e29a4b5b4aa"),  // side profile
            };

            public static readonly string[] CourtPool = {
                U("1595950653106-6c9ebd614d3a"),  // white court shoe
                U("1606107557195-0e29a4b5b4aa"),  // clean side
                U("1542291026-7eec264c27ff"),  // generic athletic
                U("1571019613454-1cb2f99b2d8b"),  // grey court style
            };

            public static readonly string[] OutdoorPool = {
                U("1520975954732-35dd22299614"),  // trail shoes on rocks
                U("1606107557195-0e29a4b5b4aa"),  // outdoor side
                U("1542291026-7eec264c27ff"),  // athletic neutral
                U("1571019613454-1cb2f99b2d8b"),  // trail-runner grey
            };

            public static string[] PoolForCategory(string categoryName) => categoryName switch
            {
                "Running" => RunningPool,
                "Training & Gym" => TrainingPool,
                "Basketball" => BasketballPool,
                "Soccer / Football" => SoccerPool,
                "Court Sports" => CourtPool,
                "Outdoor & Hiking" => OutdoorPool,
                _ => RunningPool
            };
        }

        // ============================================================
        // 1. ROLES
        // ============================================================
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // ============================================================
        // 2. DEFAULT ADMIN USER
        // ============================================================
        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            const string adminEmail = "admin@nextstep.com";
            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing != null) return;

            var admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "NextStep Admin"
            };
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // ============================================================
        // 3. BRANDS — logos served by Google's favicon CDN
        // ============================================================
        private static async Task SeedBrandsAsync(ApplicationDbContext context)
        {
            if (await context.Brands.AnyAsync()) return;

            var brands = new List<Brand>
            {
                new Brand { Name = "Nike",          Logo = Images.Logos.Nike },
                new Brand { Name = "Adidas",        Logo = Images.Logos.Adidas },
                new Brand { Name = "Puma",          Logo = Images.Logos.Puma },
                new Brand { Name = "New Balance",   Logo = Images.Logos.NewBalance },
                new Brand { Name = "Reebok",        Logo = Images.Logos.Reebok },
                new Brand { Name = "Asics",         Logo = Images.Logos.Asics },
                new Brand { Name = "Under Armour",  Logo = Images.Logos.UnderArmour },
                new Brand { Name = "Converse",      Logo = Images.Logos.Converse }
            };
            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 4. CATEGORIES
        // ============================================================
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync()) return;

            var categories = new List<Category>
            {
                new Category {
                    Name = "Running",
                    Description = "Lightweight, cushioned shoes built for road and track running.",
                    ImagePath = Images.Categories.Running
                },
                new Category {
                    Name = "Training & Gym",
                    Description = "Stable, versatile shoes for cross-training, weights, and gym workouts.",
                    ImagePath = Images.Categories.Training
                },
                new Category {
                    Name = "Basketball",
                    Description = "High-top support and grip for the court.",
                    ImagePath = Images.Categories.Basketball
                },
                new Category {
                    Name = "Soccer / Football",
                    Description = "Studded and turf shoes built for soccer/football performance.",
                    ImagePath = Images.Categories.Soccer
                },
                new Category {
                    Name = "Court Sports",
                    Description = "Tennis, badminton, and volleyball shoes with lateral support.",
                    ImagePath = Images.Categories.CourtSports
                },
                new Category {
                    Name = "Outdoor & Hiking",
                    Description = "Durable, grippy shoes for trails and rough terrain.",
                    ImagePath = Images.Categories.Outdoor
                }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 5. PRODUCTS — trimmed from ~40 to ~20 (3-4 per category)
        // Keeps the catalog feeling curated instead of bloated.
        // ============================================================
        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            var brands = await context.Brands.ToDictionaryAsync(b => b.Name, b => b.Id);
            var cats = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            var products = new List<Product>
            {
                // ---------- RUNNING (4) ----------
                new Product { Name = "Nike Air Zoom Pegasus 40",    Description = "Daily trainer with responsive Zoom Air cushioning.", Price = 130, Gender = Gender.Unisex, BrandId = brands["Nike"],        CategoryId = cats["Running"] },
                new Product { Name = "Adidas Ultraboost Light",     Description = "Boost midsole for endless energy return.",          Price = 190, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Running"] },
                new Product { Name = "Asics Gel-Kayano 30",         Description = "Premium stability for long-distance runners.",      Price = 165, Gender = Gender.Women,  BrandId = brands["Asics"],       CategoryId = cats["Running"] },
                new Product { Name = "New Balance Fresh Foam 1080", Description = "Plush Fresh Foam X for everyday miles.",            Price = 165, Gender = Gender.Unisex, BrandId = brands["New Balance"], CategoryId = cats["Running"] },
 
                // ---------- TRAINING & GYM (3) ----------
                new Product { Name = "Nike Metcon 9",               Description = "Stable platform for heavy lifts and HIIT.",         Price = 150, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Training & Gym"] },
                new Product { Name = "Reebok Nano X4",              Description = "The gym shoe — built for CrossFit.",                Price = 140, Gender = Gender.Unisex, BrandId = brands["Reebok"],      CategoryId = cats["Training & Gym"] },
                new Product { Name = "Under Armour TriBase Reign",  Description = "Low-to-ground feel for stability.",                 Price = 120, Gender = Gender.Men,    BrandId = brands["Under Armour"],CategoryId = cats["Training & Gym"] },
 
                // ---------- BASKETBALL (3) ----------
                new Product { Name = "Nike LeBron 21",              Description = "Signature LeBron with Zoom Air cushioning.",        Price = 200, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Basketball"] },
                new Product { Name = "Adidas Harden Vol. 8",        Description = "Lockdown feel for explosive moves.",                Price = 150, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Basketball"] },
                new Product { Name = "Under Armour Curry 11",       Description = "Curry's latest — light and responsive.",            Price = 160, Gender = Gender.Men,    BrandId = brands["Under Armour"],CategoryId = cats["Basketball"] },
 
                // ---------- SOCCER / FOOTBALL (3) ----------
                new Product { Name = "Nike Mercurial Vapor 15",     Description = "Speed boot for explosive wingers.",                 Price = 250, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Adidas Predator Accuracy",    Description = "Control-focused boot with rubber elements.",        Price = 220, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Puma Future 7 Ultimate",      Description = "Adaptive FUZIONFIT360 fit.",                        Price = 240, Gender = Gender.Men,    BrandId = brands["Puma"],        CategoryId = cats["Soccer / Football"] },
 
                // ---------- COURT SPORTS (3) ----------
                new Product { Name = "Asics Gel-Resolution 9",      Description = "Premium tennis shoe with stability.",               Price = 160, Gender = Gender.Men,    BrandId = brands["Asics"],       CategoryId = cats["Court Sports"] },
                new Product { Name = "Nike Court Air Zoom Vapor",   Description = "Lightweight tennis performance.",                   Price = 140, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Court Sports"] },
                new Product { Name = "Adidas Barricade 13",         Description = "Durable build for hard courts.",                    Price = 150, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Court Sports"] },
 
                // ---------- OUTDOOR & HIKING (3) ----------
                new Product { Name = "Adidas Terrex Free Hiker 2",  Description = "Boost cushioning for trails.",                      Price = 200, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "New Balance Hierro v8",       Description = "Fresh Foam cushioning meets Vibram outsole.",       Price = 140, Gender = Gender.Unisex, BrandId = brands["New Balance"], CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Nike Pegasus Trail 5",        Description = "Road-to-trail hybrid.",                             Price = 140, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Outdoor & Hiking"] }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 6. PRODUCT VARIANTS
        // ============================================================
        private static async Task SeedProductVariantsAsync(ApplicationDbContext context)
        {
            if (await context.ProductVariants.AnyAsync()) return;

            var products = await context.Products.ToListAsync();
            var variants = new List<ProductVariant>();
            var rng = new Random(42);

            var menSizes = new[] { "40", "41", "42", "43", "44", "45", "46" };
            var womenSizes = new[] { "36", "37", "38", "39", "40", "41" };
            var kidsSizes = new[] { "28", "30", "32", "34", "36" };
            var colors = new[] { "Black", "White", "Red", "Blue", "Grey", "Green" };

            foreach (var p in products)
            {
                string[] sizePool = p.Gender switch
                {
                    Gender.Kids => kidsSizes,
                    Gender.Women => womenSizes,
                    Gender.Men => menSizes,
                    _ => menSizes
                };

                var chosenColors = colors.OrderBy(_ => rng.Next()).Take(rng.Next(2, 4)).ToArray();

                foreach (var color in chosenColors)
                {
                    var chosenSizes = sizePool.OrderBy(_ => rng.Next()).Take(rng.Next(3, 6)).ToArray();
                    foreach (var size in chosenSizes)
                    {
                        var roll = rng.Next(100);
                        int stock = roll < 15 ? 0
                                  : roll < 40 ? rng.Next(1, 4)
                                  : rng.Next(5, 16);

                        variants.Add(new ProductVariant
                        {
                            ProductId = p.Id,
                            Size = size,
                            Color = color,
                            SKU = $"P{p.Id}-{color[..2].ToUpper()}-{size}",
                            StockQuantity = stock,
                            VariantPrice = p.Price
                        });
                    }
                }
            }

            await context.ProductVariants.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 7. PRODUCT IMAGES — 3 photos per product from its category pool
        // Since the pool is small (4 photos) and we pick 3, products in
        // the same category share photos but in different orderings,
        // so the catalog still looks varied without weird mismatches.
        // ============================================================
        private static async Task SeedProductImagesAsync(ApplicationDbContext context)
        {
            if (await context.ProductImages.AnyAsync()) return;

            var products = await context.Products
                .Include(p => p.Category)
                .ToListAsync();

            var images = new List<ProductImage>();

            foreach (var p in products)
            {
                var pool = Images.PoolForCategory(p.Category.Name);

                // Deterministic, distinct picks based on product Id
                var picked = new List<string>();
                var used = new HashSet<int>();
                for (int i = 0; i < 3 && used.Count < pool.Length; i++)
                {
                    int idx = (p.Id + i) % pool.Length;
                    while (used.Contains(idx))
                        idx = (idx + 1) % pool.Length;
                    used.Add(idx);
                    picked.Add(pool[idx]);
                }

                for (int i = 0; i < picked.Count; i++)
                {
                    images.Add(new ProductImage
                    {
                        ProductId = p.Id,
                        ImagePath = picked[i],
                        IsMain = i == 0
                    });
                }
            }

            await context.ProductImages.AddRangeAsync(images);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 8. DISCOUNTS
        // ============================================================
        private static async Task SeedDiscountsAsync(ApplicationDbContext context)
        {
            if (await context.Discounts.AnyAsync()) return;

            var products = await context.Products.Take(6).ToListAsync();
            var discounts = new List<Discount>();
            var rng = new Random(99);

            foreach (var p in products)
            {
                discounts.Add(new Discount
                {
                    ProductId = p.Id,
                    Title = $"Sale on {p.Name}",
                    Description = "Limited-time discount.",
                    DiscountPercentage = rng.Next(10, 41),
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(14),
                    IsActive = true
                });
            }

            await context.Discounts.AddRangeAsync(discounts);
            await context.SaveChangesAsync();
        }
    }
}