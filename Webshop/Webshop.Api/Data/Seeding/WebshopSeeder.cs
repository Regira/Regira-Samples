using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using Webshop.Api.Entities.Brands;
using Webshop.Api.Entities.Categories;
using Webshop.Api.Entities.Orders;
using Webshop.Api.Entities.Products;
using Webshop.Api.Entities.Promotions;

namespace Webshop.Api.Data.Seeding;

/// <summary>
/// Seeds the catalog and order history through the IEntityService implementations (preppers, primers,
/// normalizers and reactors all run, exactly as for API writes). Runs only on an empty database.
/// </summary>
public class WebshopSeeder(
    WebshopDbContext dbContext,
    IEntityService<Category, int> categoryService,
    IEntityService<Brand, int> brandService,
    IEntityService<Promotion, int> promotionService,
    IEntityService<Product, int> productService,
    IEntityService<Order, int> orderService,
    ILogger<WebshopSeeder> logger)
{
    public const int ProductCount = 500;
    public const int OrderCount = 350;

    public async Task Seed(CancellationToken token = default)
    {
        if (await dbContext.Categories.AnyAsync(token))
            return;

        Randomizer.Seed = new Random(20260927);
        var now = DateTime.UtcNow;
        var faker = new Faker("en");

        // wave 1: categories + brands (reference data)
        var sortOrder = 0;
        foreach (var c in CatalogData.Categories)
            await categoryService.Add(new Category
            {
                Title = c.Title, Slug = c.Slug, Icon = c.Icon, Color = c.Color, Description = c.Description,
                SortOrder = ++sortOrder, Created = now.AddDays(-420)
            }, token);
        foreach (var (name, country) in CatalogData.Categories.SelectMany(c => c.Brands).DistinctBy(b => b.Name))
            await brandService.Add(new Brand
            {
                Title = name, Country = country,
                Description = $"{name} is an independent {country} brand known for thoughtful design and dependable quality.",
                Website = $"https://www.{name.ToLowerInvariant().Replace(" & ", "and").Replace(" ", "")}.example",
                Created = now.AddDays(-420)
            }, token);
        await categoryService.SaveChanges(token);

        // reference earlier waves by key (the change tracker is cleared after every save)
        var categoryIds = await dbContext.Categories.AsNoTracking().ToDictionaryAsync(x => x.Slug, x => x.Id, token);
        var brandIds = await dbContext.Brands.AsNoTracking().ToDictionaryAsync(x => x.Title, x => x.Id, token);

        // wave 2: products (the primary entity) - title, brand, price range and features are drawn per category
        var skus = new HashSet<string>();
        var titles = new HashSet<string>();
        for (var i = 0; i < ProductCount; i++)
        {
            var cat = CatalogData.Categories[i % CatalogData.Categories.Length];
            var type = faker.PickRandom(cat.Types);
            var brand = faker.PickRandom(cat.Brands);
            string title;
            do
            {
                var series = faker.PickRandom(CatalogData.Series) + (faker.Random.Bool(0.5f) ? $" {faker.Random.Int(2, 7)}" : "");
                title = $"{brand.Name} {series} {type.Name}";
            } while (!titles.Add(title)); // unique product names
            var price = RoundPrice(faker.Random.Decimal(type.MinPrice, type.MaxPrice));
            var onSale = faker.Random.Bool(0.22f);
            var reviewCount = faker.Random.Bool(0.08f) ? 0 : (int)Math.Round(Math.Pow(faker.Random.Double(), 2.2) * 2400) + 1;
            var stockRoll = faker.Random.Double();
            var features = faker.PickRandom(type.Features, 3).ToArray();
            string sku;
            do sku = $"{cat.Slug[..3].ToUpperInvariant()}-{brand.Name[..2].ToUpperInvariant()}-{faker.Random.Int(1000, 9999)}";
            while (!skus.Add(sku));

            await productService.Add(new Product
            {
                Title = title,
                Sku = sku,
                Description = $"The {title} features {features[0]}, {features[1]} and {features[2]}. "
                              + $"Designed by {brand.Name} in {brand.Country} and backed by a 2-year warranty.",
                Price = price,
                CompareAtPrice = onSale ? RoundPrice(price * faker.Random.Decimal(1.15m, 1.5m)) : null,
                Stock = stockRoll < 0.07 ? 0 : stockRoll < 0.2 ? faker.Random.Int(1, 5) : faker.Random.Int(8, 160),
                ReviewCount = reviewCount,
                Rating = reviewCount == 0 ? 0 : Math.Round(Math.Min(5, faker.Random.Double(3.1, 4.6) + faker.Random.Double(0, 0.6)), 1),
                IsFeatured = faker.Random.Bool(0.07f),
                IsActive = faker.Random.Bool(0.97f),
                CategoryId = categoryIds[cat.Slug],
                BrandId = brandIds[brand.Name],
                Created = now.AddDays(-faker.Random.Double(1, 400))
            }, token);
        }
        await productService.SaveChanges(token);

        // wave 3: promotions (storefront banners)
        var promotions = new (string Title, string Subtitle, string Badge, string Cta, string Link, string Theme, string Icon, int From, int To, bool Active)[]
        {
            ("Summer Sound Sale", "Save up to 30% on headphones, speakers and soundbars.", "-30%", "Shop audio", $"/shop?categoryId={categoryIds["audio"]}&onSale=true", "sunset", "headphones", -10, 20, true),
            ("Smarter home, simpler life", "Starter kits, cameras and thermostats that just work together.", "NEW", "Explore smart home", $"/shop?categoryId={categoryIds["smart-home"]}", "ocean", "house-gear", -30, 60, true),
            ("Free shipping over \u20ac50", "Standard delivery is on us for every order above \u20ac50.", "FREE", "Start shopping", "/shop", "forest", "truck", -90, 180, true),
            ("Kitchen Week", "Espresso machines, air fryers and chef knives at their best prices.", "DEALS", "Shop kitchen", $"/shop?categoryId={categoryIds["kitchen"]}", "berry", "cup-hot", -3, 11, true),
            ("Level up your setup", "Top-rated gaming gear, rated 4 stars and up by players.", "TOP RATED", "Shop gaming", $"/shop?categoryId={categoryIds["gaming"]}&minRating=4", "midnight", "controller", -5, 25, true),
            ("Spring Outdoor Event", "Trail shoes, tents and packs for the new season.", "ENDED", "Shop outdoors", $"/shop?categoryId={categoryIds["sports-outdoors"]}", "forest", "bicycle", -150, -100, true),
            ("Black Friday preview", "A sneak peek at our biggest sale of the year.", "SOON", "Get notified", "/shop", "midnight", "lightning", 50, 60, false)
        };
        var promoOrder = 0;
        foreach (var p in promotions)
            await promotionService.Add(new Promotion
            {
                Title = p.Title, Subtitle = p.Subtitle, Badge = p.Badge, CtaLabel = p.Cta, CtaLink = p.Link, Theme = p.Theme,
                Icon = p.Icon, StartDate = now.Date.AddDays(p.From), EndDate = now.Date.AddDays(p.To), IsActive = p.Active,
                SortOrder = ++promoOrder, Created = now.AddDays(Math.Min(p.From, 0) - 7)
            }, token);
        await promotionService.SaveChanges(token);

        // wave 4: orders - statuses follow each order's age so the pipeline looks realistic
        var sellable = await dbContext.Products.AsNoTracking()
            .Where(p => p.IsActive && p.Stock >= 3)
            .Select(p => new { p.Id, p.ReviewCount })
            .ToListAsync(token);
        var weightedIds = sellable.Select(p => p.Id).ToArray();
        var weights = sellable.Select(p => (float)(p.ReviewCount + 50)).ToArray();
        var weightSum = weights.Sum();
        weights = weights.Select(w => w / weightSum).ToArray();

        for (var i = 0; i < OrderCount; i++)
        {
            var ageDays = Math.Pow(faker.Random.Double(), 1.6) * 180; // more recent orders than old ones
            var created = now.AddDays(-ageDays);
            var destination = faker.PickRandom(CatalogData.Destinations);
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var lineCount = faker.Random.WeightedRandom([1, 2, 3, 4], [0.45f, 0.3f, 0.17f, 0.08f]);
            var productIds = new HashSet<int>();
            while (productIds.Count < lineCount)
                productIds.Add(faker.Random.WeightedRandom(weightedIds, weights));

            await orderService.Add(new Order
            {
                Status = StatusForAge(faker, ageDays),
                CustomerName = $"{first} {last}",
                Email = faker.Internet.Email(first, last),
                Phone = faker.Random.Bool(0.7f) ? faker.Random.ReplaceNumbers(destination.PhoneFormat) : null,
                Street = $"{faker.Address.StreetName()} {faker.Random.Int(1, 240)}",
                PostalCode = faker.Random.ReplaceNumbers(new string('#', destination.PostalDigits)).TrimStart('0').PadLeft(destination.PostalDigits, '1'),
                City = faker.PickRandom(destination.Cities),
                Country = destination.Country,
                ShippingMethod = faker.Random.WeightedRandom([ShippingMethod.Standard, ShippingMethod.Express, ShippingMethod.Pickup], [0.72f, 0.2f, 0.08f]),
                PaymentMethod = faker.Random.WeightedRandom([PaymentMethod.Card, PaymentMethod.PayPal, PaymentMethod.BankTransfer, PaymentMethod.CashOnDelivery], [0.55f, 0.25f, 0.12f, 0.08f]),
                Notes = faker.Random.Bool(0.12f) ? faker.PickRandom("Please leave the parcel with the neighbours.", "Gift - please don't include the invoice.", "Ring twice, the doorbell is quiet.", "Deliver after 5pm if possible.") : null,
                OrderLines = productIds.Select(id => new OrderLine
                {
                    ProductId = id,
                    Quantity = faker.Random.WeightedRandom([1, 2, 3], [0.75f, 0.2f, 0.05f])
                }).ToList(),
                Created = created
            }, token);
        }
        await orderService.SaveChanges(token);

        logger.LogInformation("Seeded {Categories} categories, {Brands} brands, {Products} products, {Promotions} promotions and {Orders} orders",
            categoryIds.Count, brandIds.Count, ProductCount, promotions.Length, OrderCount);
    }

    /// <summary>Retail-style price: 24.99, 129.99, 1049.00 ...</summary>
    private static decimal RoundPrice(decimal value)
        => value >= 500 ? Math.Round(value / 10m) * 10m - 1m : Math.Floor(value) + 0.99m;

    private static OrderStatus StatusForAge(Faker faker, double ageDays)
    {
        if (faker.Random.Bool(0.05f)) return OrderStatus.Cancelled;
        return ageDays switch
        {
            < 0.5 => faker.Random.WeightedRandom([OrderStatus.Pending, OrderStatus.Paid], [0.6f, 0.4f]),
            < 2 => faker.Random.WeightedRandom([OrderStatus.Pending, OrderStatus.Paid, OrderStatus.Processing], [0.15f, 0.45f, 0.4f]),
            < 5 => faker.Random.WeightedRandom([OrderStatus.Processing, OrderStatus.Shipped], [0.3f, 0.7f]),
            < 9 => faker.Random.WeightedRandom([OrderStatus.Shipped, OrderStatus.Delivered], [0.35f, 0.65f]),
            _ => OrderStatus.Delivered
        };
    }
}
