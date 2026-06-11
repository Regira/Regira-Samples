using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using Webshop.API.Data;
using Webshop.API.Entities.Categories;
using Webshop.API.Entities.Customers;
using Webshop.API.Entities.Orders;
using Webshop.API.Entities.Products;

namespace Webshop.API.Seeding;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var dbContext = sp.GetRequiredService<WebshopDbContext>();
        if (await dbContext.Orders.AnyAsync()) return;

        var categoryService = sp.GetRequiredService<IEntityService<Category>>();
        var productService = sp.GetRequiredService<IEntityService<Product>>();
        var customerService = sp.GetRequiredService<IEntityService<Customer, Guid>>();
        var orderService = sp.GetRequiredService<IEntityService<Order>>();

        // Categories
        var categories = new[]
        {
            new Category { Title = "Electronics", Description = "Devices, gadgets, and accessories" },
            new Category { Title = "Clothing", Description = "Fashion for men, women, and children" },
            new Category { Title = "Home & Garden", Description = "Everything for your home and outdoor spaces" },
            new Category { Title = "Sports & Outdoors", Description = "Gear and equipment for active lifestyles" },
            new Category { Title = "Books", Description = "Fiction, non-fiction, and educational titles" },
            new Category { Title = "Toys & Games", Description = "Fun for all ages" },
            new Category { Title = "Health & Beauty", Description = "Wellness and personal care products" },
            new Category { Title = "Food & Beverages", Description = "Gourmet foods and drinks" },
        };

        foreach (var cat in categories)
            await categoryService.Add(cat);
        await categoryService.SaveChanges();

        // Products (5-6 per category = ~45 products)
        var products = new[]
        {
            // Electronics
            new Product { Title = "Wireless Noise-Cancelling Headphones", Description = "Premium over-ear headphones with 30h battery", Price = 249.99m, StockQuantity = 50 },
            new Product { Title = "4K Smart TV 55\"", Description = "Ultra HD display with built-in streaming apps", Price = 699.99m, StockQuantity = 20 },
            new Product { Title = "Mechanical Keyboard", Description = "TKL layout with RGB backlighting", Price = 129.99m, StockQuantity = 75 },
            new Product { Title = "USB-C Hub 7-in-1", Description = "Expand your laptop connectivity", Price = 49.99m, StockQuantity = 100 },
            new Product { Title = "Portable Bluetooth Speaker", Description = "Waterproof with 20h playtime", Price = 89.99m, StockQuantity = 60 },
            new Product { Title = "Smartwatch Series 5", Description = "Health monitoring and GPS tracking", Price = 199.99m, StockQuantity = 40 },

            // Clothing
            new Product { Title = "Classic Denim Jacket", Description = "Timeless blue denim, relaxed fit", Price = 79.99m, StockQuantity = 80 },
            new Product { Title = "Running Shorts", Description = "Lightweight with inner liner", Price = 34.99m, StockQuantity = 120 },
            new Product { Title = "Wool Blend Sweater", Description = "Cosy merino wool, multiple colours", Price = 64.99m, StockQuantity = 90 },
            new Product { Title = "Slim Fit Chinos", Description = "Versatile everyday trousers", Price = 54.99m, StockQuantity = 110 },
            new Product { Title = "Waterproof Rain Jacket", Description = "Packable and breathable", Price = 89.99m, StockQuantity = 55 },

            // Home & Garden
            new Product { Title = "Bamboo Cutting Board Set", Description = "Set of 3 sizes with juice grooves", Price = 39.99m, StockQuantity = 70 },
            new Product { Title = "Stainless Steel Cookware Set", Description = "10-piece set, dishwasher safe", Price = 189.99m, StockQuantity = 30 },
            new Product { Title = "Memory Foam Pillow", Description = "Ergonomic cervical support", Price = 44.99m, StockQuantity = 85 },
            new Product { Title = "Smart LED Bulb 4-Pack", Description = "Wi-Fi enabled, 16 million colours", Price = 34.99m, StockQuantity = 150 },
            new Product { Title = "Herb Garden Starter Kit", Description = "Seeds, pots, and soil for 6 herbs", Price = 24.99m, StockQuantity = 65 },

            // Sports & Outdoors
            new Product { Title = "Yoga Mat Premium", Description = "6mm thick, non-slip, eco-friendly", Price = 49.99m, StockQuantity = 95 },
            new Product { Title = "Adjustable Dumbbell Set", Description = "5–52 lb per dumbbell, quick-change", Price = 299.99m, StockQuantity = 25 },
            new Product { Title = "Hiking Backpack 45L", Description = "Waterproof with integrated rain cover", Price = 119.99m, StockQuantity = 40 },
            new Product { Title = "Cycling Helmet", Description = "MIPS certified, ventilated", Price = 79.99m, StockQuantity = 55 },
            new Product { Title = "Foam Roller Set", Description = "3-piece recovery kit", Price = 29.99m, StockQuantity = 90 },

            // Books
            new Product { Title = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship", Price = 34.99m, StockQuantity = 200 },
            new Product { Title = "The Pragmatic Programmer", Description = "Your journey to mastery, 20th anniversary edition", Price = 39.99m, StockQuantity = 180 },
            new Product { Title = "Designing Data-Intensive Applications", Description = "The big ideas behind reliable, scalable systems", Price = 44.99m, StockQuantity = 160 },
            new Product { Title = "Atomic Habits", Description = "An easy and proven way to build good habits", Price = 19.99m, StockQuantity = 250 },
            new Product { Title = "Sapiens", Description = "A brief history of humankind", Price = 17.99m, StockQuantity = 220 },

            // Toys & Games
            new Product { Title = "LEGO Architecture Set", Description = "900-piece landmark building set", Price = 69.99m, StockQuantity = 45 },
            new Product { Title = "Strategy Board Game", Description = "2–6 players, 60-minute play time", Price = 44.99m, StockQuantity = 60 },
            new Product { Title = "Remote Control Car", Description = "1:16 scale, 30 km/h top speed", Price = 54.99m, StockQuantity = 50 },
            new Product { Title = "Wooden Puzzle 1000 pcs", Description = "Scenic landscape design", Price = 24.99m, StockQuantity = 80 },
            new Product { Title = "Educational Science Kit", Description = "20 experiments for kids aged 8+", Price = 34.99m, StockQuantity = 70 },

            // Health & Beauty
            new Product { Title = "Electric Toothbrush", Description = "Sonic technology, 4 modes", Price = 59.99m, StockQuantity = 100 },
            new Product { Title = "Vitamin D3 + K2 Supplement", Description = "3-month supply, 2000 IU", Price = 19.99m, StockQuantity = 300 },
            new Product { Title = "Natural Face Moisturiser", Description = "SPF 30, suitable for all skin types", Price = 29.99m, StockQuantity = 120 },
            new Product { Title = "Resistance Bands Set", Description = "5 levels, latex-free", Price = 22.99m, StockQuantity = 150 },
            new Product { Title = "Sleep Mask & Ear Plugs Set", Description = "Contoured design, hypoallergenic", Price = 14.99m, StockQuantity = 200 },

            // Food & Beverages
            new Product { Title = "Specialty Coffee Beans 500g", Description = "Single-origin, medium roast", Price = 16.99m, StockQuantity = 180 },
            new Product { Title = "Assorted Tea Collection", Description = "48 premium tea bags, 12 varieties", Price = 12.99m, StockQuantity = 220 },
            new Product { Title = "Dark Chocolate Selection Box", Description = "12 artisan bars, 70–90% cocoa", Price = 28.99m, StockQuantity = 90 },
            new Product { Title = "Organic Honey 500g", Description = "Raw, unfiltered wildflower honey", Price = 14.99m, StockQuantity = 130 },
            new Product { Title = "Hot Sauce Variety Pack", Description = "6 bottles, mild to extra hot", Price = 22.99m, StockQuantity = 110 },
        };

        int[] categoryIds = categories.Select(c => c.Id).ToArray();
        var productCategoryMap = new Dictionary<int, int[]>
        {
            // Electronics (index 0) → products 0-5
            [0] = [0, 1, 2, 3, 4, 5],
            // Clothing (index 1) → products 6-10
            [1] = [6, 7, 8, 9, 10],
            // Home & Garden (index 2) → products 11-15
            [2] = [11, 12, 13, 14, 15],
            // Sports & Outdoors (index 3) → products 16-20
            [3] = [16, 17, 18, 19, 20],
            // Books (index 4) → products 21-25
            [4] = [21, 22, 23, 24, 25],
            // Toys & Games (index 5) → products 26-30
            [5] = [26, 27, 28, 29, 30],
            // Health & Beauty (index 6) → products 31-35
            [6] = [31, 32, 33, 34, 35],
            // Food & Beverages (index 7) → products 36-40
            [7] = [36, 37, 38, 39, 40],
        };

        foreach (var (catIndex, productIndices) in productCategoryMap)
        {
            foreach (var pIndex in productIndices)
            {
                products[pIndex].Categories = [new ProductCategory { CategoryId = categoryIds[catIndex] }];
            }
        }

        foreach (var product in products)
            await productService.Add(product);
        await productService.SaveChanges();

        // Customers (20 customers)
        var customers = new Customer[]
        {
            new() { Name = "Alice Johnson", Email = "alice.johnson@example.com", Phone = "+32 470 123 001" },
            new() { Name = "Bob Smith", Email = "bob.smith@example.com", Phone = "+32 470 123 002" },
            new() { Name = "Carol White", Email = "carol.white@example.com", Phone = "+32 470 123 003" },
            new() { Name = "David Brown", Email = "david.brown@example.com", Phone = "+32 470 123 004" },
            new() { Name = "Emma Davis", Email = "emma.davis@example.com", Phone = "+32 470 123 005" },
            new() { Name = "Frank Wilson", Email = "frank.wilson@example.com", Phone = "+32 470 123 006" },
            new() { Name = "Grace Martinez", Email = "grace.martinez@example.com", Phone = "+32 470 123 007" },
            new() { Name = "Henry Taylor", Email = "henry.taylor@example.com", Phone = "+32 470 123 008" },
            new() { Name = "Isla Anderson", Email = "isla.anderson@example.com", Phone = "+32 470 123 009" },
            new() { Name = "Jack Thomas", Email = "jack.thomas@example.com", Phone = "+32 470 123 010" },
            new() { Name = "Karen Jackson", Email = "karen.jackson@example.com", Phone = "+32 470 123 011" },
            new() { Name = "Liam Harris", Email = "liam.harris@example.com", Phone = "+32 470 123 012" },
            new() { Name = "Mia Clark", Email = "mia.clark@example.com", Phone = "+32 470 123 013" },
            new() { Name = "Noah Lewis", Email = "noah.lewis@example.com", Phone = "+32 470 123 014" },
            new() { Name = "Olivia Walker", Email = "olivia.walker@example.com", Phone = "+32 470 123 015" },
            new() { Name = "Peter Hall", Email = "peter.hall@example.com", Phone = "+32 470 123 016" },
            new() { Name = "Quinn Allen", Email = "quinn.allen@example.com", Phone = "+32 470 123 017" },
            new() { Name = "Rachel Young", Email = "rachel.young@example.com", Phone = "+32 470 123 018" },
            new() { Name = "Samuel King", Email = "samuel.king@example.com", Phone = "+32 470 123 019" },
            new() { Name = "Tina Wright", Email = "tina.wright@example.com", Phone = "+32 470 123 020" },
        };

        foreach (var customer in customers)
            await customerService.Add(customer);
        await customerService.SaveChanges();

        // Build 102 orders distributed across customers and products
        var rng = new Random(42);
        var productList = products.ToList();
        var customerList = customers.ToList();
        var statuses = Enum.GetValues<OrderStatus>();
        var cities = new[] { "Brussels", "Antwerp", "Ghent", "Bruges", "Leuven", "Liège", "Namur", "Mechelen" };

        int orderCount = 0;
        foreach (var customer in customerList)
        {
            int ordersForCustomer = rng.Next(3, 8);
            for (int o = 0; o < ordersForCustomer && orderCount < 102; o++)
            {
                var city = cities[rng.Next(cities.Length)];
                var createdDaysAgo = rng.Next(1, 365);
                var status = statuses[rng.Next(statuses.Length)];
                int lineCount = rng.Next(1, 5);

                var selectedProducts = productList
                    .OrderBy(_ => rng.Next())
                    .Take(lineCount)
                    .ToList();

                var orderLines = selectedProducts.Select((p, i) => new OrderLine
                {
                    ProductId = p.Id,
                    Quantity = rng.Next(1, 4),
                    UnitPrice = p.Price,
                    SortOrder = i
                }).ToList();

                foreach (var line in orderLines)
                    line.SubTotal = line.Quantity * line.UnitPrice;

                var order = new Order
                {
                    CustomerId = customer.Id,
                    Status = status,
                    ShippingAddress = $"{rng.Next(1, 200)} Main Street, {city}",
                    Created = DateTime.UtcNow.AddDays(-createdDaysAgo),
                    OrderLines = orderLines,
                    Total = orderLines.Sum(l => l.SubTotal)
                };

                await orderService.Add(order);
                orderCount++;
            }
        }

        await orderService.SaveChanges();
    }
}
