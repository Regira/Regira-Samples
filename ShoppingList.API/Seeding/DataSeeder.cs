using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using ShoppingListApi.Data;
using ShoppingListApi.Entities.Articles;
using ShoppingListApi.Entities.Categories;
using ShoppingListApi.Entities.Lists;
using ShoppingListApi.Entities.Shoppers;

namespace ShoppingListApi.Seeding;

/// <summary>
/// Seeds sample data through the Regira <see cref="IEntityService{TEntity, TKey}"/> implementations
/// (not raw EF inserts), so primers, normalizers and Related() collection sync all run.
/// </summary>
public static class DataSeeder
{
    private const int ArticleCount = 500;

    /// <summary>Two-level grocery taxonomy: root category → child categories.</summary>
    private static readonly Dictionary<string, string[]> Taxonomy = new()
    {
        ["Fruit & Vegetables"] = ["Fruit", "Vegetables", "Herbs", "Salads"],
        ["Dairy & Eggs"] = ["Milk", "Cheese", "Yoghurt", "Eggs", "Butter"],
        ["Bakery"] = ["Bread", "Pastry", "Cakes"],
        ["Meat & Fish"] = ["Beef", "Poultry", "Pork", "Fish", "Seafood"],
        ["Pantry"] = ["Pasta & Rice", "Canned Goods", "Sauces", "Spices", "Baking"],
        ["Beverages"] = ["Water", "Soft Drinks", "Juice", "Coffee & Tea", "Beer & Wine"],
        ["Frozen"] = ["Frozen Vegetables", "Ice Cream", "Frozen Meals", "Frozen Fish"],
        ["Snacks & Sweets"] = ["Crisps", "Chocolate", "Biscuits", "Nuts"],
        ["Household"] = ["Cleaning", "Laundry", "Paper Goods", "Kitchen"],
        ["Personal Care"] = ["Hygiene", "Hair Care", "Oral Care", "Skin Care"],
        ["Baby"] = ["Diapers", "Baby Food"],
        ["Pets"] = ["Dog", "Cat"]
    };

    private static readonly string[] ListTitles =
    [
        "Weekly groceries", "Weekend BBQ", "Party supplies", "Office snacks",
        "Camping trip", "Birthday dinner", "Quick run", "Monthly stock-up"
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ShoppingListDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DataSeeder");

        if (await db.Articles.AnyAsync())
        {
            logger.LogInformation("Database already contains data — skipping seed.");
            return;
        }

        // Deterministic sample data across runs.
        Randomizer.Seed = new Random(20260614);
        var rnd = new Random(20260614);

        var categoryService = services.GetRequiredService<IEntityService<Category, int>>();
        var articleService = services.GetRequiredService<IEntityService<Article, int>>();
        var shopperService = services.GetRequiredService<IEntityService<Shopper, int>>();
        var listService = services.GetRequiredService<IEntityService<ShoppingList, int>>();

        var categories = await SeedCategories(categoryService, logger);
        var articles = await SeedArticles(articleService, categories, rnd, logger);
        var shoppers = await SeedShoppers(shopperService, logger);
        var listCount = await SeedLists(listService, shoppers, articles, rnd, logger);

        logger.LogInformation(
            "Seeding complete: {Categories} categories, {Articles} articles, {Shoppers} shoppers, {Lists} lists.",
            categories.Count, articles.Count, shoppers.Count, listCount);
    }

    private static async Task<List<Category>> SeedCategories(IEntityService<Category, int> service, ILogger logger)
    {
        // Wave 1: root categories — saved first so their auto-increment Ids are available.
        var roots = new Dictionary<string, Category>();
        foreach (var rootName in Taxonomy.Keys)
        {
            var root = new Category { Title = rootName, Description = $"{rootName} products" };
            await service.Add(root);
            roots[rootName] = root;
        }
        await service.SaveChanges();

        // Wave 2: child categories — each linked to its root via a RelatedCategory (synced by Related()).
        var all = new List<Category>(roots.Values);
        foreach (var (rootName, children) in Taxonomy)
        {
            var root = roots[rootName];
            foreach (var childName in children)
            {
                var child = new Category
                {
                    Title = childName,
                    Description = $"{childName} in {rootName}",
                    ParentEntities = [new RelatedCategory { ParentId = root.Id }]
                };
                await service.Add(child);
                all.Add(child);
            }
        }
        await service.SaveChanges();

        logger.LogInformation("Seeded {Count} categories.", all.Count);
        return all;
    }

    private static async Task<List<Article>> SeedArticles(
        IEntityService<Article, int> service, List<Category> categories, Random rnd, ILogger logger)
    {
        var faker = new Faker<Article>()
            .RuleFor(a => a.Title, f => f.Commerce.ProductName())
            .RuleFor(a => a.Description, f => f.Commerce.ProductDescription())
            .RuleFor(a => a.Brand, f => f.Company.CompanyName())
            .RuleFor(a => a.Unit, f => f.PickRandom("piece", "pack", "L", "ml", "kg", "g", "bottle", "box"));

        var articles = new List<Article>(ArticleCount);
        for (var i = 0; i < ArticleCount; i++)
        {
            var article = faker.Generate();

            // Assign 1-3 distinct categories per article (synced through the Article service).
            var categoryCount = rnd.Next(1, 4);
            article.Categories = categories
                .OrderBy(_ => rnd.Next())
                .Take(categoryCount)
                .Select(c => new ArticleCategory { CategoryId = c.Id })
                .ToList();

            await service.Add(article);
            articles.Add(article);

            if ((i + 1) % 100 == 0)
                await service.SaveChanges();
        }
        await service.SaveChanges();

        logger.LogInformation("Seeded {Count} articles.", articles.Count);
        return articles;
    }

    private static async Task<List<Shopper>> SeedShoppers(IEntityService<Shopper, int> service, ILogger logger)
    {
        var faker = new Faker<Shopper>()
            .RuleFor(s => s.Name, f => f.Name.FullName())
            .RuleFor(s => s.Email, (f, s) => f.Internet.Email(s.Name));

        var shoppers = faker.Generate(15);

        // Guarantee unique emails (DB has a unique index).
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var suffix = 1;
        foreach (var shopper in shoppers)
        {
            while (!seen.Add(shopper.Email))
                shopper.Email = $"{suffix++}.{shopper.Email}";
            await service.Add(shopper);
        }
        await service.SaveChanges();

        logger.LogInformation("Seeded {Count} shoppers.", shoppers.Count);
        return shoppers;
    }

    private static async Task<int> SeedLists(
        IEntityService<ShoppingList, int> service, List<Shopper> shoppers, List<Article> articles, Random rnd, ILogger logger)
    {
        var listCount = 0;
        foreach (var shopper in shoppers)
        {
            var listsForShopper = rnd.Next(1, 4); // 1-3 lists per shopper
            for (var i = 0; i < listsForShopper; i++)
            {
                var itemCount = rnd.Next(5, 21); // 5-20 items per list
                var items = articles
                    .OrderBy(_ => rnd.Next())
                    .Take(itemCount)
                    .Select((a, idx) => new ShoppingListItem
                    {
                        ArticleId = a.Id,
                        Quantity = rnd.Next(1, 6),
                        IsActive = rnd.NextDouble() > 0.3, // ~70% active
                        SortOrder = idx
                    })
                    .ToList();

                var list = new ShoppingList
                {
                    Title = ListTitles[rnd.Next(ListTitles.Length)],
                    ShopperId = shopper.Id,
                    Items = items
                };
                await service.Add(list);
                listCount++;
            }
        }
        await service.SaveChanges();

        logger.LogInformation("Seeded {Count} shopping lists.", listCount);
        return listCount;
    }
}
