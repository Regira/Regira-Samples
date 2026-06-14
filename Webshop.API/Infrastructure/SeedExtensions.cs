using Regira.Entities.Services.Abstractions;
using Webshop.API.Entities.Categories;
using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Orders;
using Webshop.API.Entities.Products;

namespace Webshop.API.Infrastructure;

public static class SeedExtensions
{
    public static async Task SeedDataAsync(this IServiceProvider services)
    {
        var categoryService = services.GetRequiredService<IEntityService<Category, int>>();
        if (await categoryService.Count(null) > 0) return;

        // ── 1. Categories ───────────────────────────────────────────────
        var categoryNames = new[]
        {
            "Electronics", "Clothing", "Books", "Home & Garden",
            "Sports & Outdoors", "Toys & Games", "Health & Beauty", "Food & Drink",
        };
        var categories = categoryNames.Select(name => new Category { Title = name }).ToList();
        foreach (var cat in categories) await categoryService.Add(cat);
        await categoryService.SaveChanges();

        // ── 2. Products ─────────────────────────────────────────────────
        var productService = services.GetRequiredService<IEntityService<Product, int>>();

        int ElecId() => categories[0].Id;
        int ClothId() => categories[1].Id;
        int BookId() => categories[2].Id;
        int HomeId() => categories[3].Id;
        int SportId() => categories[4].Id;
        int HealthId() => categories[6].Id;

        var productDefs = new (string Title, string Description, decimal Price, int Stock, int[] CatIds)[]
        {
            ("Wireless Headphones", "Premium noise-cancelling over-ear headphones with 30h battery.", 89.99m, 45, [ElecId()]),
            ("Smartphone Stand", "Adjustable aluminium desk stand for phones and tablets.", 19.99m, 120, [ElecId()]),
            ("4K Smart TV 55\"", "55-inch 4K UHD Smart TV with HDR and built-in streaming.", 599.99m, 18, [ElecId()]),
            ("USB-C Hub 7-in-1", "Compact hub with HDMI, USB-A, SD card reader and more.", 34.99m, 80, [ElecId()]),
            ("Bluetooth Speaker", "Waterproof portable speaker with 360-degree sound.", 49.99m, 60, [ElecId()]),

            ("Classic White T-Shirt", "100% organic cotton unisex tee, available in S-XL.", 14.99m, 200, [ClothId()]),
            ("Slim Fit Jeans", "Dark wash stretch denim, slim-fit cut.", 39.99m, 150, [ClothId()]),
            ("Running Jacket", "Lightweight windproof jacket with reflective strips.", 59.99m, 75, [ClothId(), SportId()]),
            ("Wool Sweater", "Merino wool knit sweater, ideal for autumn.", 49.99m, 90, [ClothId()]),
            ("Leather Sneakers", "Minimalist leather sneakers with cushioned sole.", 79.99m, 55, [ClothId()]),

            ("Clean Code", "Robert C. Martin's guide to writing readable, maintainable code.", 29.99m, 40, [BookId()]),
            ("The Pragmatic Programmer", "Classic guide for software craftsmen.", 34.99m, 35, [BookId()]),
            ("Atomic Habits", "Build good habits and break bad ones with small changes.", 17.99m, 65, [BookId()]),
            ("Designing Data-Intensive Applications", "In-depth guide to distributed systems.", 44.99m, 28, [BookId()]),
            ("Deep Work", "Strategies for focused success in a distracted world.", 15.99m, 50, [BookId()]),

            ("Plant Pot Set (3pc)", "Ceramic plant pots in terracotta finish, set of three.", 24.99m, 110, [HomeId()]),
            ("LED Desk Lamp", "Touch-dimmer LED lamp with USB charging port.", 32.99m, 70, [HomeId(), ElecId()]),
            ("Bamboo Cutting Board", "Extra-large bamboo cutting board with juice groove.", 22.99m, 95, [HomeId()]),
            ("Scented Candle Set", "Set of 4 soy-wax candles in seasonal scents.", 27.99m, 85, [HomeId()]),
            ("Memory Foam Pillow", "Ergonomic memory-foam pillow with cooling cover.", 39.99m, 60, [HomeId()]),

            ("Yoga Mat", "Non-slip eco-friendly TPE yoga mat, 6mm thick.", 29.99m, 140, [SportId()]),
            ("Resistance Bands Set", "5-piece latex resistance band set for home workouts.", 18.99m, 160, [SportId()]),
            ("Cycling Helmet", "Lightweight MIPS-certified road cycling helmet.", 64.99m, 40, [SportId()]),
            ("Water Bottle 1L", "Stainless steel double-wall insulated sports bottle.", 24.99m, 180, [SportId()]),
            ("Foam Roller", "High-density foam roller for muscle recovery.", 21.99m, 100, [SportId(), HealthId()]),
        };

        var products = productDefs.Select(d => new Product
        {
            Title = d.Title,
            Description = d.Description,
            Price = d.Price,
            Stock = d.Stock,
            Categories = d.CatIds.Select((cid, idx) => new ProductCategory { CategoryId = cid }).ToList()
        }).ToList();

        foreach (var p in products) await productService.Add(p);
        await productService.SaveChanges();

        // ── 3. Customers ────────────────────────────────────────────────
        var customerService = services.GetRequiredService<IEntityService<Customer, Guid>>();

        var customerDefs = new (string Name, string Email, string? Phone, string? Address)[]
        {
            ("Alice Johnson", "alice.johnson@example.com", "+1-202-555-0101", "101 Maple St, Springfield, IL 62701"),
            ("Bob Martinez", "bob.martinez@example.com", "+1-202-555-0102", "22 Oak Ave, Portland, OR 97201"),
            ("Carol White", "carol.white@example.com", "+1-202-555-0103", "5 Pine Rd, Austin, TX 73301"),
            ("David Lee", "david.lee@example.com", "+1-202-555-0104", "88 Birch Blvd, Denver, CO 80201"),
            ("Eva Green", "eva.green@example.com", "+1-202-555-0105", "14 Cedar Ln, Seattle, WA 98101"),
            ("Frank Brown", "frank.brown@example.com", "+1-202-555-0106", "73 Elm St, Boston, MA 02101"),
            ("Grace Kim", "grace.kim@example.com", "+1-202-555-0107", "30 Walnut Dr, Miami, FL 33101"),
            ("Henry Davis", "henry.davis@example.com", "+1-202-555-0108", "66 Spruce Ct, Chicago, IL 60601"),
            ("Iris Wilson", "iris.wilson@example.com", "+1-202-555-0109", "9 Ash Way, Phoenix, AZ 85001"),
            ("Jack Taylor", "jack.taylor@example.com", "+1-202-555-0110", "55 Poplar Pl, Nashville, TN 37201"),
        };

        var customers = customerDefs.Select(c => new Customer
        {
            Name = c.Name,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address,
        }).ToList();

        foreach (var c in customers) await customerService.Add(c);
        // No SaveChanges needed — Guid IDs are generated by the Prepare lambda during Add()

        // ── 4. Orders (~110 orders) ─────────────────────────────────────
        var orderService = services.GetRequiredService<IEntityService<Order, int>>();
        var rng = new Random(42);
        var statuses = Enum.GetValues<OrderStatus>();

        var orderTemplates = new (int[] ProductIndices, int[] Quantities)[]
        {
            ([0, 3], [1, 1]),
            ([1, 15], [2, 1]),
            ([5, 6, 8], [3, 2, 1]),
            ([10, 12], [1, 1]),
            ([20, 21], [1, 2]),
            ([2], [1]),
            ([7, 22], [1, 1]),
            ([4, 16], [1, 1]),
            ([11, 13], [1, 1]),
            ([17, 18, 19], [1, 2, 1]),
            ([9, 5], [1, 2]),
            ([23, 24], [2, 1]),
            ([14], [1]),
            ([0, 4], [1, 1]),
            ([6, 8, 9], [2, 1, 1]),
            ([1, 3], [3, 1]),
            ([10, 11], [1, 1]),
            ([20, 24], [2, 1]),
            ([15, 16], [2, 1]),
            ([12, 13, 14], [1, 1, 1]),
            ([2, 3], [1, 2]),
            ([21, 22, 23], [3, 1, 2]),
            ([5, 7], [4, 1]),
            ([18, 19], [1, 3]),
            ([0], [2]),
            ([6, 9, 8], [1, 1, 2]),
            ([17, 20], [1, 2]),
            ([1, 5, 6], [2, 1, 1]),
            ([11, 12], [1, 2]),
            ([4, 23, 24], [1, 2, 1]),
        };

        int orderCount = 0;
        var orderDate = DateTime.UtcNow.AddDays(-180);

        foreach (var customer in customers)
        {
            var ordersPerCustomer = 10 + rng.Next(3);
            for (var i = 0; i < ordersPerCustomer; i++)
            {
                var template = orderTemplates[orderCount % orderTemplates.Length];
                var lines = template.ProductIndices
                    .Select((pi, idx) =>
                    {
                        var product = products[pi];
                        var qty = template.Quantities[idx];
                        return new OrderLine
                        {
                            ProductId = product.Id,
                            Quantity = qty,
                            UnitPrice = product.Price,
                        };
                    })
                    .ToList();

                var order = new Order
                {
                    CustomerId = customer.Id,
                    Status = statuses[rng.Next(statuses.Length)],
                    OrderLines = lines,
                };

                await orderService.Add(order);
                orderCount++;
            }
        }

        await orderService.SaveChanges();
    }
}
