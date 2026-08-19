using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using Regira.Utilities;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Data.Seeding;

/// <summary>
/// Seeds ShopMate with a demo catalog: a category hierarchy (some categories with more than one
/// parent), a set of shopping lists per shopper, and ~500 articles distributed across them.
/// Runs through the registered IEntityService implementations, never straight against the DbSet,
/// so the same primers/normalizers/Related() sync a real request would go through also apply here.
/// </summary>
public static class DataSeeder
{
    private sealed record CategorySeed(string Title, string Icon, string ColorHex, string[] ParentTitles, string[]? Items = null, string? Unit = null);

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopMateDbContext>();

        // Idempotent: only seed an empty database (EnsureCreated() never migrates an existing one).
        if (await dbContext.Categories.AnyAsync())
        {
            return;
        }

        var categoryService = scope.ServiceProvider.GetRequiredService<IEntityService<Category, int>>();
        var shoppingListService = scope.ServiceProvider.GetRequiredService<IEntityService<ShoppingList, int>>();
        var articleService = scope.ServiceProvider.GetRequiredService<IEntityService<Article, int>>();

        var random = new Random(20260819);

        // --- 1. Categories --------------------------------------------------------------------
        var categorySeeds = BuildCategorySeeds();

        var categoryByTitle = new Dictionary<string, int>();
        foreach (var seed in categorySeeds)
        {
            var category = new Category { Title = seed.Title, Icon = seed.Icon, ColorHex = seed.ColorHex };
            await categoryService.Add(category);
            categoryByTitle[seed.Title] = 0; // filled in after SaveChanges below
        }
        await categoryService.SaveChanges();

        // Re-read (the tracker cleared on save) to resolve titles -> ids for the relation wave below.
        var savedCategories = await dbContext.Categories.AsNoTracking().ToListAsync();
        foreach (var c in savedCategories)
        {
            categoryByTitle[c.Title] = c.Id;
        }

        // Owned join rows (RelatedCategory) have no service of their own and no single owning parent
        // here (both sides already exist), so they go straight on the DbContext.
        var seenLinks = new HashSet<(int ParentId, int ChildId)>();
        foreach (var seed in categorySeeds)
        {
            foreach (var parentTitle in seed.ParentTitles)
            {
                var link = (ParentId: categoryByTitle[parentTitle], ChildId: categoryByTitle[seed.Title]);
                if (seenLinks.Add(link))
                {
                    dbContext.RelatedCategories.Add(new RelatedCategory { ParentId = link.ParentId, ChildId = link.ChildId });
                }
            }
        }
        await dbContext.SaveChangesAsync();

        // --- 2. Shopping lists ------------------------------------------------------------------
        var shoppingListSeeds = BuildShoppingListSeeds();
        var listFaker = new Faker();
        var shoppingLists = new List<ShoppingList>();
        foreach (var (title, icon, colorHex, isArchived) in shoppingListSeeds)
        {
            var owner = listFaker.Name.FullName();
            var list = new ShoppingList
            {
                Title = title,
                Icon = icon,
                ColorHex = colorHex,
                OwnerName = owner,
                Description = listFaker.Lorem.Sentence(6, 4),
                IsArchived = isArchived,
                Created = listFaker.Date.Past(1, DateTime.UtcNow.AddDays(-1)).AsUtc(),
            };
            list.LastModified = listFaker.Date.Between(list.Created, DateTime.UtcNow).AsUtc();
            shoppingLists.Add(list);
            await shoppingListService.Add(list);
        }
        await shoppingListService.SaveChanges();
        var listIds = shoppingLists.Select(l => l.Id).ToList();

        // --- 3. Articles (~500, the primary entity) ---------------------------------------------
        var itemPools = categorySeeds
            .Where(s => s.Items is { Length: > 0 })
            .ToDictionary(s => s.Title, s => s);
        var poolTitles = itemPools.Keys.ToList();

        const int targetArticleCount = 500;
        var articleFaker = new Faker();
        var sortOrderByList = listIds.ToDictionary(id => id, _ => 0);

        for (var i = 0; i < targetArticleCount; i++)
        {
            var poolTitle = poolTitles[random.Next(poolTitles.Count)];
            var pool = itemPools[poolTitle];
            var itemName = pool.Items![random.Next(pool.Items!.Length)];
            var listId = listIds[random.Next(listIds.Count)];
            var isActive = random.NextDouble() < 0.65; // ~65% still need to buy, ~35% already bought

            var created = articleFaker.Date.Past(1, DateTime.UtcNow.AddDays(-1)).AsUtc();
            DateTime? lastModified = random.NextDouble() < 0.5
                ? articleFaker.Date.Between(created, DateTime.UtcNow).AsUtc()
                : null;

            var categoryIds = new List<int> { categoryByTitle[poolTitle] };
            // Occasionally also tag the item's parent (a coarser, cross-cutting category) so a shopper
            // filtering by "Produce" also finds items filed under the more specific "Fruits"/"Vegetables".
            var parentTitle = categorySeeds.First(s => s.Title == poolTitle).ParentTitles.FirstOrDefault();
            if (parentTitle != null && random.NextDouble() < 0.3)
            {
                categoryIds.Add(categoryByTitle[parentTitle]);
            }

            var article = new Article
            {
                Title = itemName,
                Notes = random.NextDouble() < 0.15 ? articleFaker.Lorem.Sentence(4, 3) : null,
                Quantity = RandomQuantity(random, pool.Unit),
                Unit = pool.Unit,
                IsActive = isActive,
                SortOrder = sortOrderByList[listId]++,
                ShoppingListId = listId,
                Created = created,
                LastModified = lastModified,
                Categories = categoryIds.Distinct().Select(cid => new ArticleCategory { CategoryId = cid }).ToList(),
            };

            await articleService.Add(article);

            // Preppers run per item; save in reasonably sized batches rather than one giant transaction.
            if ((i + 1) % 50 == 0)
            {
                await articleService.SaveChanges();
            }
        }
        await articleService.SaveChanges();
    }

    private static decimal RandomQuantity(Random random, string? unit)
    {
        return unit switch
        {
            "kg" or "L" => Math.Round((decimal)(random.NextDouble() * 2.5 + 0.25), 2),
            _ => random.Next(1, 6)
        };
    }

    private static List<CategorySeed> BuildCategorySeeds()
    {
        var seeds = new List<CategorySeed>
        {
            // Root categories
            new("Produce", "bi-apple", "#4caf50", []),
            new("Dairy & Eggs", "bi-egg", "#ffca28", []),
            new("Bakery", "bi-bread-slice", "#c8a165", []),
            new("Meat & Seafood", "bi-egg-fried", "#e57373", []),
            new("Pantry", "bi-basket", "#8d6e63", []),
            new("Frozen", "bi-snow", "#4fc3f7", []),
            new("Beverages", "bi-cup-straw", "#ba68c8", []),
            new("Household", "bi-house", "#78909c", []),
            new("Personal Care", "bi-droplet", "#f06292", []),
            new("Snacks", "bi-cookie", "#ffb74d", []),
            new("Baby", "bi-balloon", "#90caf9", []),
            new("Pet Supplies", "bi-heart", "#a1887f", []),

            // Children (single parent)
            new("Fruits", "bi-apple", "#66bb6a", ["Produce"], ["Apples", "Bananas", "Grapes", "Lemons", "Avocados", "Oranges", "Strawberries", "Blueberries"], "pcs"),
            new("Vegetables", "bi-flower1", "#81c784", ["Produce"], ["Carrots", "Spinach", "Tomatoes", "Potatoes", "Onions", "Bell Peppers", "Broccoli", "Garlic", "Mushrooms", "Cucumbers"], "pcs"),
            new("Milk & Cream", "bi-cup", "#fff176", ["Dairy & Eggs"], ["Whole Milk", "Skim Milk", "Heavy Cream", "Sour Cream", "Half & Half"], "L"),
            new("Cheese", "bi-pie-chart", "#ffd54f", ["Dairy & Eggs"], ["Cheddar Cheese", "Mozzarella", "Cream Cheese", "Parmesan", "Cottage Cheese"], "pack"),
            new("Eggs", "bi-egg-fill", "#ffe082", ["Dairy & Eggs"], ["Free-Range Eggs", "Organic Eggs", "Egg Whites"], "pack"),
            new("Bread & Rolls", "bi-bread-slice", "#d7b98a", ["Bakery"], ["Sourdough Bread", "Whole Wheat Bread", "Bagels", "Dinner Rolls", "Baguette", "Tortillas"], "pcs"),
            new("Pastries", "bi-cake", "#e0c097", ["Bakery"], ["Croissants", "Muffins", "Cinnamon Rolls", "Donuts"], "pcs"),
            new("Fresh Meat", "bi-egg-fried", "#ef9a9a", ["Meat & Seafood"], ["Chicken Breast", "Ground Beef", "Pork Chops", "Turkey Slices", "Bacon", "Sausages"], "kg"),
            new("Seafood", "bi-water", "#80cbc4", ["Meat & Seafood"], ["Salmon Fillet", "Shrimp", "Tuna Steaks", "Cod Fillet"], "kg"),
            new("Canned Goods", "bi-archive", "#a1887f", ["Pantry"], ["Canned Tomatoes", "Black Beans", "Canned Tuna", "Canned Corn", "Chickpeas"], "can"),
            new("Grains & Pasta", "bi-egg", "#bcaaa4", ["Pantry"], ["Rice", "Spaghetti", "Penne Pasta", "Cereal", "Oats", "Couscous"], "pack"),
            new("Baking", "bi-cup-hot", "#d7ccc8", ["Pantry"], ["Flour", "Sugar", "Baking Powder", "Vanilla Extract", "Chocolate Chips"], "pack"),
            new("Condiments", "bi-droplet-half", "#bcaaa4", ["Pantry"], ["Olive Oil", "Ketchup", "Mustard", "Mayonnaise", "Soy Sauce", "Honey", "Peanut Butter"], "bottle"),
            new("Frozen Meals", "bi-snow2", "#81d4fa", ["Frozen"], ["Frozen Pizza", "Frozen Lasagna", "Frozen Burritos"], "pcs"),
            new("Frozen Produce", "bi-snow3", "#4dd0e1", ["Frozen"], ["Frozen Peas", "Frozen Berries", "Frozen Fries", "Frozen Corn"], "bag"),
            new("Ice Cream", "bi-snow", "#b3e5fc", ["Frozen"], ["Vanilla Ice Cream", "Chocolate Ice Cream", "Sorbet"], "pack"),
            new("Hot Drinks", "bi-cup-hot", "#8d6e63", ["Beverages"], ["Coffee Beans", "Ground Coffee", "Tea Bags", "Hot Chocolate Mix"], "pack"),
            new("Cold Drinks", "bi-cup-straw", "#ce93d8", ["Beverages"], ["Orange Juice", "Sparkling Water", "Soda", "Lemonade", "Apple Juice"], "bottle"),
            new("Alcohol", "bi-cup", "#9575cd", ["Beverages"], ["Red Wine", "White Wine", "Craft Beer"], "bottle"),
            new("Cleaning", "bi-droplet", "#90a4ae", ["Household"], ["Dish Soap", "Laundry Detergent", "All-Purpose Cleaner", "Sponges", "Trash Bags"], "pcs"),
            new("Paper Goods", "bi-file-earmark", "#b0bec5", ["Household"], ["Paper Towels", "Toilet Paper", "Aluminum Foil", "Napkins"], "pack"),
            new("Bath & Body", "bi-droplet", "#f48fb1", ["Personal Care"], ["Shampoo", "Body Wash", "Soap Bar", "Sunscreen"], "bottle"),
            new("Oral & Health", "bi-heart-pulse", "#f06292", ["Personal Care"], ["Toothpaste", "Toothbrush", "Deodorant", "Vitamins"], "pcs"),
            new("Sweet Snacks", "bi-cookie", "#ffcc80", ["Snacks"], ["Chocolate Bar", "Granola Bars", "Cookies", "Fruit Gummies"], "pack"),
            new("Savory Snacks", "bi-egg-fried", "#ffb74d", ["Snacks"], ["Potato Chips", "Pretzels", "Popcorn", "Mixed Nuts", "Crackers"], "bag"),
            new("Baby Feeding", "bi-cup-straw", "#90caf9", ["Baby"], ["Baby Formula", "Baby Food Jars", "Baby Cereal"], "pcs"),
            new("Baby Care", "bi-balloon", "#64b5f6", ["Baby"], ["Diapers", "Baby Wipes", "Baby Lotion"], "pack"),
            new("Dog Supplies", "bi-heart", "#a1887f", ["Pet Supplies"], ["Dog Food", "Dog Treats", "Dog Toys"], "pack"),
            new("Cat Supplies", "bi-heart-fill", "#8d6e63", ["Pet Supplies"], ["Cat Food", "Cat Litter", "Cat Treats"], "pack"),

            // Multi-parent categories: a child filed under two different roots at once.
            new("Organic", "bi-flower2", "#66bb6a", ["Produce", "Dairy & Eggs"]),
            new("Bulk Buy", "bi-boxes", "#8d6e63", ["Pantry", "Household"]),
        };

        return seeds;
    }

    private static List<(string Title, string Icon, string ColorHex, bool IsArchived)> BuildShoppingListSeeds() =>
    [
        ("Weekly Groceries", "bi-cart", "#4caf50", false),
        ("Weekend BBQ", "bi-fire", "#e57373", false),
        ("Office Snacks", "bi-building", "#ffb74d", false),
        ("Camping Trip", "bi-tree", "#66bb6a", false),
        ("Birthday Party", "bi-balloon", "#ba68c8", false),
        ("Meal Prep Sunday", "bi-calendar-week", "#4fc3f7", false),
        ("Movie Night", "bi-film", "#9575cd", false),
        ("Holiday Dinner", "bi-gift", "#ef5350", false),
        ("Baby Essentials", "bi-heart", "#90caf9", false),
        ("New Apartment", "bi-house-door", "#78909c", false),
        ("Game Day Snacks", "bi-trophy", "#ffa726", false),
        ("Sunday Brunch", "bi-cup-hot", "#ffca28", false),
        ("Road Trip Supplies", "bi-signpost", "#4db6ac", false),
        ("Pet Shopping", "bi-heart-fill", "#a1887f", false),
        ("Quick Top-Up", "bi-lightning", "#ff8a65", false),
        ("Vegan Week", "bi-flower1", "#81c784", false),
        ("Gym & Protein Stock", "bi-activity", "#7986cb", false),
        ("Book Club Snacks", "bi-book", "#f06292", false),
        ("Dorm Room Restock", "bi-mortarboard", "#64b5f6", false),
        ("Neighborhood Potluck", "bi-people", "#aed581", false),
        ("Fall Cleaning Supplies", "bi-brush", "#90a4ae", false),
        ("Kids Lunchbox", "bi-basket2", "#ffd54f", false),
        ("Old Apartment (moved out)", "bi-box-seam", "#bdbdbd", true),
        ("Cancelled Dinner Party", "bi-x-circle", "#e0e0e0", true),
        ("Last Month's Groceries", "bi-clock-history", "#cfd8dc", true),
    ];
}
