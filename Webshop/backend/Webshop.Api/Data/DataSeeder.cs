using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using Webshop.Api.Entities.Categories;
using Webshop.Api.Entities.Orders;
using Webshop.Api.Entities.Products;

namespace Webshop.Api.Data;

public static class DataSeeder
{
    private static readonly (string Title, string Description, string Image)[] CategorySeed =
    [
        ("Electronics", "Phones, laptops, audio and smart gadgets for everyday life.", "electronics"),
        ("Home & Kitchen", "Cookware, small appliances and decor to upgrade your home.", "home"),
        ("Fashion", "Clothing, shoes and accessories for every season.", "fashion"),
        ("Sports & Outdoors", "Gear for training, hiking, camping and team sports.", "sports"),
        ("Beauty & Personal Care", "Skincare, haircare and grooming essentials.", "beauty"),
        ("Toys & Games", "Fun for kids and adults, from puzzles to building sets.", "toys"),
        ("Books & Media", "Bestsellers, comics and educational reads.", "books"),
        ("Office & Stationery", "Everything for a productive home office.", "office"),
        ("Garden & Outdoor", "Tools, furniture and decor for your garden.", "garden"),
        ("Pet Supplies", "Food, toys and accessories for your best friend.", "pets"),
        ("Automotive", "Car care, accessories and tools.", "auto"),
        ("Groceries", "Pantry staples, snacks and beverages.", "groceries"),
        ("Jewelry & Watches", "Timepieces and jewelry for every occasion.", "jewelry"),
        ("Baby & Kids", "Everything for newborns, toddlers and young kids.", "baby")
    ];

    private static readonly string[] Countries = ["Belgium", "Netherlands", "Germany", "France", "Luxembourg"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var db = sp.GetRequiredService<WebshopDbContext>();

        if (await db.Categories.AnyAsync())
            return; // already seeded

        var random = new Random(20260819);
        Randomizer.Seed = random;

        var categoryService = sp.GetRequiredService<IEntityService<Category, int>>();
        var productService = sp.GetRequiredService<IEntityService<Product, int>>();
        var orderService = sp.GetRequiredService<IEntityService<Order, int>>();

        // ---- Wave 1: Categories ----
        var categories = new List<Category>();
        for (var i = 0; i < CategorySeed.Length; i++)
        {
            var (title, description, imageSeed) = CategorySeed[i];
            var category = new Category
            {
                Title = title,
                Slug = Slugify(title),
                Description = description,
                ImageUrl = $"https://picsum.photos/seed/webshop-cat-{imageSeed}/800/500",
                DisplayOrder = i,
                IsFeatured = i < 6,
                Created = DateTime.UtcNow.AddDays(-400 + i)
            };
            categories.Add(category);
            await categoryService.Add(category);
        }
        await categoryService.SaveChanges();

        // ---- Wave 2: Products (~500) ----
        const int productCount = 500;
        var brands = new[]
        {
            "Northwind", "Zenlyte", "Vertex", "Boreal", "Cascade", "Lumino", "Kestrel", "Marlow",
            "Ashgrove", "Pinehall", "Solace", "Redwick", "Havenly", "Crestline", "Amberwood"
        };
        var slugs = new HashSet<string>();
        var products = new List<Product>();

        var titleFaker = new Faker();
        for (var i = 0; i < productCount; i++)
        {
            var category = categories[i % categories.Count];
            var brand = brands[random.Next(brands.Length)];
            var adjective = titleFaker.Commerce.ProductAdjective();
            var material = titleFaker.Commerce.ProductMaterial();
            var noun = titleFaker.Commerce.ProductName();
            var title = $"{adjective} {material} {noun}".Trim();
            // Keep title within the 128-char column limit.
            if (title.Length > 120) title = title[..120];

            var baseSlug = Slugify($"{brand}-{title}");
            var slug = baseSlug;
            var suffix = 1;
            while (!slugs.Add(slug))
                slug = $"{baseSlug}-{suffix++}";

            var price = Math.Round((decimal)(4.99 + random.NextDouble() * 895.0), 2);
            var onSale = random.NextDouble() < 0.22;
            var compareAt = onSale ? Math.Round(price * (decimal)(1.15 + random.NextDouble() * 0.5), 2) : (decimal?)null;
            var stock = random.NextDouble() < 0.08 ? 0 : random.Next(1, 250);
            var rating = Math.Round(2.8 + random.NextDouble() * 2.2, 1);
            var reviewCount = random.Next(0, 480);
            // Spread Created across the last ~10 months so "newest" sort and any recency badge
            // aren't a 0%/100% bucket.
            var created = DateTime.UtcNow.AddDays(-random.Next(0, 300)).AddHours(-random.Next(0, 24));

            var product = new Product
            {
                Title = title,
                Slug = slug,
                Description = titleFaker.Commerce.ProductDescription(),
                Code = $"SKU-{(i + 1):D5}",
                CategoryId = category.Id,
                Brand = brand,
                ImageUrl = $"https://picsum.photos/seed/webshop-prod-{i + 1}/600/600",
                Price = price,
                CompareAtPrice = compareAt,
                Stock = stock,
                Rating = (decimal)rating,
                ReviewCount = reviewCount,
                IsFeatured = random.NextDouble() < 0.1,
                Created = created
            };
            products.Add(product);
            await productService.Add(product);
        }
        await productService.SaveChanges();

        // ---- Wave 3: Orders (~150), each with 1-5 lines ----
        const int orderCount = 150;
        var statusWeights = new (OrderStatus Status, double Weight)[]
        {
            (OrderStatus.Pending, 0.12),
            (OrderStatus.Processing, 0.15),
            (OrderStatus.Shipped, 0.18),
            (OrderStatus.Delivered, 0.45),
            (OrderStatus.Cancelled, 0.10)
        };

        for (var i = 0; i < orderCount; i++)
        {
            var person = new Person();
            var lineCount = random.Next(1, 6);
            var lines = new List<OrderLine>();
            var usedProductIds = new HashSet<int>();
            for (var l = 0; l < lineCount; l++)
            {
                Product product;
                var attempts = 0;
                do
                {
                    product = products[random.Next(products.Count)];
                    attempts++;
                } while (!usedProductIds.Add(product.Id) && attempts < 10);

                lines.Add(new OrderLine
                {
                    ProductId = product.Id,
                    Quantity = random.Next(1, 4)
                });
            }

            // Order Created is spread over the last ~8 months; status skews toward "further along"
            // for older orders so the distribution reads as plausible, not uniform-random noise.
            var ageDays = random.Next(0, 240);
            var created = DateTime.UtcNow.AddDays(-ageDays).AddHours(-random.Next(0, 24));
            var status = PickStatus(statusWeights, random, ageDays);

            var order = new Order
            {
                CustomerName = person.FullName,
                CustomerEmail = person.Email,
                CustomerPhone = person.Phone,
                ShippingAddress = $"{person.Address.Street} {random.Next(1, 250)}",
                ShippingCity = person.Address.City,
                ShippingPostalCode = person.Address.ZipCode,
                ShippingCountry = Countries[random.Next(Countries.Length)],
                Status = status,
                OrderLines = lines,
                Created = created
            };
            await orderService.Add(order);
        }
        await orderService.SaveChanges();
    }

    private static OrderStatus PickStatus((OrderStatus Status, double Weight)[] weights, Random random, int ageDays)
    {
        // Very recent orders skew toward Pending/Processing; older ones skew toward Delivered/Cancelled.
        var adjusted = weights.Select(w => w.Status switch
        {
            OrderStatus.Pending or OrderStatus.Processing when ageDays < 5 => (Status: w.Status, Weight: w.Weight * 4),
            OrderStatus.Delivered when ageDays > 20 => (Status: w.Status, Weight: w.Weight * 1.6),
            _ => (Status: w.Status, Weight: w.Weight)
        }).ToArray();

        var total = adjusted.Sum(w => w.Weight);
        var roll = random.NextDouble() * total;
        var cumulative = 0.0;
        foreach (var (status, weight) in adjusted)
        {
            cumulative += weight;
            if (roll <= cumulative) return status;
        }
        return OrderStatus.Delivered;
    }

    private static string Slugify(string value)
    {
        var lowered = value.ToLowerInvariant();
        var chars = lowered.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var slug = new string(chars);
        while (slug.Contains("--"))
            slug = slug.Replace("--", "-");
        return slug.Trim('-');
    }
}
