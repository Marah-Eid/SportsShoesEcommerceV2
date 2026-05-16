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
            // Make sure DB is migrated
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
        // 1. ROLES — Admin & Customer
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
        // Email: admin@nextstep.com   Password: Admin@123
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
        // 3. BRANDS — 8 brands
        // ============================================================
        private static async Task SeedBrandsAsync(ApplicationDbContext context)
        {
            if (await context.Brands.AnyAsync()) return;

            var brands = new List<Brand>
            {
                new Brand { Name = "Nike",          Logo = "https://placehold.co/200x100/000000/FFFFFF?text=Nike" },
                new Brand { Name = "Adidas",        Logo = "https://placehold.co/200x100/000000/FFFFFF?text=Adidas" },
                new Brand { Name = "Puma",          Logo = "https://placehold.co/200x100/000000/FFFFFF?text=Puma" },
                new Brand { Name = "New Balance",   Logo = "https://placehold.co/200x100/CC0000/FFFFFF?text=New+Balance" },
                new Brand { Name = "Reebok",        Logo = "https://placehold.co/200x100/000080/FFFFFF?text=Reebok" },
                new Brand { Name = "Asics",         Logo = "https://placehold.co/200x100/0066CC/FFFFFF?text=Asics" },
                new Brand { Name = "Under Armour",  Logo = "https://placehold.co/200x100/000000/FFFFFF?text=Under+Armour" },
                new Brand { Name = "Converse",      Logo = "https://placehold.co/200x100/B22222/FFFFFF?text=Converse" }
            };
            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 4. CATEGORIES — 6 categories
        // ============================================================
        private static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (await context.Categories.AnyAsync()) return;

            var categories = new List<Category>
            {
                new Category {
                    Name = "Running",
                    Description = "Lightweight, cushioned shoes built for road and track running.",
                    ImagePath = "https://placehold.co/600x400/FF6B35/FFFFFF?text=Running"
                },
                new Category {
                    Name = "Training & Gym",
                    Description = "Stable, versatile shoes for cross-training, weights, and gym workouts.",
                    ImagePath = "https://placehold.co/600x400/2E86AB/FFFFFF?text=Training+%26+Gym"
                },
                new Category {
                    Name = "Basketball",
                    Description = "High-top support and grip for the court.",
                    ImagePath = "https://placehold.co/600x400/A23B72/FFFFFF?text=Basketball"
                },
                new Category {
                    Name = "Soccer / Football",
                    Description = "Studded and turf shoes built for soccer/football performance.",
                    ImagePath = "https://placehold.co/600x400/4CAF50/FFFFFF?text=Soccer+%2F+Football"
                },
                new Category {
                    Name = "Court Sports",
                    Description = "Tennis, badminton, and volleyball shoes with lateral support.",
                    ImagePath = "https://placehold.co/600x400/F18F01/FFFFFF?text=Court+Sports"
                },
                new Category {
                    Name = "Outdoor & Hiking",
                    Description = "Durable, grippy shoes for trails and rough terrain.",
                    ImagePath = "https://placehold.co/600x400/6B4226/FFFFFF?text=Outdoor+%26+Hiking"
                }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 5. PRODUCTS — ~40 products across categories/brands
        // ============================================================
        private static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (await context.Products.AnyAsync()) return;

            // Lookup brand & category IDs
            var brands = await context.Brands.ToDictionaryAsync(b => b.Name, b => b.Id);
            var cats = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            var products = new List<Product>
            {
                // ---------- RUNNING ----------
                new Product { Name = "Nike Air Zoom Pegasus 40",    Description = "Daily trainer with responsive Zoom Air cushioning.", Price = 130, Gender = Gender.Unisex, BrandId = brands["Nike"],        CategoryId = cats["Running"] },
                new Product { Name = "Adidas Ultraboost Light",     Description = "Boost midsole for endless energy return.",          Price = 190, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Running"] },
                new Product { Name = "Asics Gel-Kayano 30",         Description = "Premium stability for long-distance runners.",      Price = 165, Gender = Gender.Women,  BrandId = brands["Asics"],       CategoryId = cats["Running"] },
                new Product { Name = "New Balance Fresh Foam 1080", Description = "Plush Fresh Foam X for everyday miles.",            Price = 165, Gender = Gender.Unisex, BrandId = brands["New Balance"], CategoryId = cats["Running"] },
                new Product { Name = "Puma Velocity Nitro 3",       Description = "Lightweight Nitro foam for tempo runs.",            Price = 110, Gender = Gender.Men,    BrandId = brands["Puma"],        CategoryId = cats["Running"] },
                new Product { Name = "Nike Pegasus Kids",           Description = "Kids' version of the iconic Pegasus.",              Price = 75,  Gender = Gender.Kids,   BrandId = brands["Nike"],        CategoryId = cats["Running"] },
                new Product { Name = "Asics Novablast 4",           Description = "Bouncy ride for fun, energetic runs.",              Price = 140, Gender = Gender.Women,  BrandId = brands["Asics"],       CategoryId = cats["Running"] },

                // ---------- TRAINING & GYM ----------
                new Product { Name = "Nike Metcon 9",               Description = "Stable platform for heavy lifts and HIIT.",         Price = 150, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Training & Gym"] },
                new Product { Name = "Reebok Nano X4",              Description = "The gym shoe — built for CrossFit.",                Price = 140, Gender = Gender.Unisex, BrandId = brands["Reebok"],      CategoryId = cats["Training & Gym"] },
                new Product { Name = "Under Armour TriBase Reign",  Description = "Low-to-ground feel for stability.",                 Price = 120, Gender = Gender.Men,    BrandId = brands["Under Armour"],CategoryId = cats["Training & Gym"] },
                new Product { Name = "Adidas Dropset Trainer 2",    Description = "Wide base, locked-in feel for lifting.",            Price = 120, Gender = Gender.Women,  BrandId = brands["Adidas"],      CategoryId = cats["Training & Gym"] },
                new Product { Name = "Puma Fuse 3.0",               Description = "Versatile trainer for mixed workouts.",             Price = 100, Gender = Gender.Unisex, BrandId = brands["Puma"],        CategoryId = cats["Training & Gym"] },
                new Product { Name = "Nike Free Metcon 5",          Description = "Flexible forefoot with stable heel.",               Price = 120, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Training & Gym"] },

                // ---------- BASKETBALL ----------
                new Product { Name = "Nike LeBron 21",              Description = "Signature LeBron with Zoom Air cushioning.",        Price = 200, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Basketball"] },
                new Product { Name = "Adidas Harden Vol. 8",        Description = "Lockdown feel for explosive moves.",                Price = 150, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Basketball"] },
                new Product { Name = "Nike KD 17",                  Description = "Smooth ride with Cushlon foam.",                    Price = 150, Gender = Gender.Unisex, BrandId = brands["Nike"],        CategoryId = cats["Basketball"] },
                new Product { Name = "Puma MB.03",                  Description = "LaMelo Ball's signature, bold and bouncy.",         Price = 125, Gender = Gender.Men,    BrandId = brands["Puma"],        CategoryId = cats["Basketball"] },
                new Product { Name = "Nike Giannis Immortality 3",  Description = "Affordable performance hooper.",                    Price = 75,  Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Basketball"] },
                new Product { Name = "Under Armour Curry 11",       Description = "Curry's latest — light and responsive.",            Price = 160, Gender = Gender.Men,    BrandId = brands["Under Armour"],CategoryId = cats["Basketball"] },

                // ---------- SOCCER / FOOTBALL ----------
                new Product { Name = "Nike Mercurial Vapor 15",     Description = "Speed boot for explosive wingers.",                 Price = 250, Gender = Gender.Men,    BrandId = brands["Nike"],        CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Adidas Predator Accuracy",    Description = "Control-focused boot with rubber elements.",        Price = 220, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Puma Future 7 Ultimate",      Description = "Adaptive FUZIONFIT360 fit.",                        Price = 240, Gender = Gender.Men,    BrandId = brands["Puma"],        CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Nike Phantom GX",             Description = "Precision strikes with Gripknit upper.",            Price = 230, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Adidas Copa Pure 2",          Description = "Premium leather for the purists.",                  Price = 200, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Soccer / Football"] },
                new Product { Name = "Puma King Pro Kids",          Description = "Classic King silhouette for young players.",        Price = 65,  Gender = Gender.Kids,   BrandId = brands["Puma"],        CategoryId = cats["Soccer / Football"] },

                // ---------- COURT SPORTS ----------
                new Product { Name = "Asics Gel-Resolution 9",      Description = "Premium tennis shoe with stability.",               Price = 160, Gender = Gender.Men,    BrandId = brands["Asics"],       CategoryId = cats["Court Sports"] },
                new Product { Name = "Nike Court Air Zoom Vapor",   Description = "Lightweight tennis performance.",                   Price = 140, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Court Sports"] },
                new Product { Name = "Adidas Barricade 13",         Description = "Durable build for hard courts.",                    Price = 150, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Court Sports"] },
                new Product { Name = "Asics Sky Elite FF 2",        Description = "Volleyball shoe with FlyteFoam cushioning.",        Price = 140, Gender = Gender.Women,  BrandId = brands["Asics"],       CategoryId = cats["Court Sports"] },
                new Product { Name = "Puma Solarsmash RCT",         Description = "Badminton & racquet sports.",                       Price = 110, Gender = Gender.Unisex, BrandId = brands["Puma"],        CategoryId = cats["Court Sports"] },
                new Product { Name = "New Balance Fresh Foam Lav v2",Description = "Comfortable tennis shoe for all-day play.",         Price = 130, Gender = Gender.Men,    BrandId = brands["New Balance"], CategoryId = cats["Court Sports"] },

                // ---------- OUTDOOR & HIKING ----------
                new Product { Name = "Adidas Terrex Free Hiker 2",  Description = "Boost cushioning for trails.",                      Price = 200, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Asics Gel-Trabuco 12",        Description = "Trail-running with serious grip.",                  Price = 150, Gender = Gender.Women,  BrandId = brands["Asics"],       CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "New Balance Hierro v8",       Description = "Fresh Foam cushioning meets Vibram outsole.",       Price = 140, Gender = Gender.Unisex, BrandId = brands["New Balance"], CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Nike Pegasus Trail 5",        Description = "Road-to-trail hybrid.",                             Price = 140, Gender = Gender.Women,  BrandId = brands["Nike"],        CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Adidas Terrex Swift R3",      Description = "Lightweight hiker for fast moves.",                 Price = 160, Gender = Gender.Men,    BrandId = brands["Adidas"],      CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Puma Voyage Nitro 3",         Description = "Trail trainer with Nitro foam.",                    Price = 130, Gender = Gender.Unisex, BrandId = brands["Puma"],        CategoryId = cats["Outdoor & Hiking"] },
                new Product { Name = "Reebok Trailgrip Kids",       Description = "Kid-sized trail explorer.",                         Price = 60,  Gender = Gender.Kids,   BrandId = brands["Reebok"],      CategoryId = cats["Outdoor & Hiking"] }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 6. PRODUCT VARIANTS — sizes/colors/stock for each product
        // Mix of stock levels so filtering/UX shows realistically:
        //   - some 0 (out of stock)
        //   - some 1-3 (low stock)
        //   - some 5-15 (normal)
        // ============================================================
        private static async Task SeedProductVariantsAsync(ApplicationDbContext context)
        {
            if (await context.ProductVariants.AnyAsync()) return;

            var products = await context.Products.ToListAsync();
            var variants = new List<ProductVariant>();
            var rng = new Random(42); // fixed seed for repeatable test data

            // Pools by gender
            var menSizes = new[] { "40", "41", "42", "43", "44", "45", "46" };
            var womenSizes = new[] { "36", "37", "38", "39", "40", "41" };
            var kidsSizes = new[] { "28", "30", "32", "34", "36" };
            var colors = new[] { "Black", "White", "Red", "Blue", "Grey", "Green" };

            foreach (var p in products)
            {
                // Pick the right size pool
                string[] sizePool = p.Gender switch
                {
                    Gender.Kids => kidsSizes,
                    Gender.Women => womenSizes,
                    Gender.Men => menSizes,
                    _ => menSizes // Unisex uses men's sizes
                };

                // 2-3 colors per product
                var chosenColors = colors.OrderBy(_ => rng.Next()).Take(rng.Next(2, 4)).ToArray();

                // 3-5 sizes per color
                foreach (var color in chosenColors)
                {
                    var chosenSizes = sizePool.OrderBy(_ => rng.Next()).Take(rng.Next(3, 6)).ToArray();
                    foreach (var size in chosenSizes)
                    {
                        // Stock distribution: 15% out, 25% low, 60% normal
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
                            VariantPrice = p.Price // same as base; admin can adjust later
                        });
                    }
                }
            }

            await context.ProductVariants.AddRangeAsync(variants);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 7. PRODUCT IMAGES — 3 images per product, first is main
        // Using placeholder URLs — replace with real images later
        // ============================================================
        private static async Task SeedProductImagesAsync(ApplicationDbContext context)
        {
            if (await context.ProductImages.AnyAsync()) return;

            var products = await context.Products.ToListAsync();
            var images = new List<ProductImage>();

            // Color palette per category just to vary placeholders
            string ColorForCategory(int catId) => (catId % 6) switch
            {
                0 => "FF6B35",
                1 => "2E86AB",
                2 => "A23B72",
                3 => "4CAF50",
                4 => "F18F01",
                _ => "6B4226"
            };

            foreach (var p in products)
            {
                var color = ColorForCategory(p.CategoryId);
                var nameEncoded = Uri.EscapeDataString(p.Name);

                images.Add(new ProductImage
                {
                    ProductId = p.Id,
                    ImagePath = $"https://placehold.co/600x600/{color}/FFFFFF?text={nameEncoded}",
                    IsMain = true
                });
                images.Add(new ProductImage
                {
                    ProductId = p.Id,
                    ImagePath = $"https://placehold.co/600x600/333333/FFFFFF?text={nameEncoded}+Side",
                    IsMain = false
                });
                images.Add(new ProductImage
                {
                    ProductId = p.Id,
                    ImagePath = $"https://placehold.co/600x600/777777/FFFFFF?text={nameEncoded}+Back",
                    IsMain = false
                });
            }

            await context.ProductImages.AddRangeAsync(images);
            await context.SaveChangesAsync();
        }

        // ============================================================
        // 8. DISCOUNTS — 6 active discounts on random products
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
                    DiscountPercentage = rng.Next(10, 41), // 10%–40%
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
