using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;
using ShoppingList.API.Data;
using ShoppingList.API.Entities.Articles;
using ShoppingList.API.Entities.Categories;
using ShoppingList.API.Entities.Shoppers;
using ShoppingList.API.Entities.ShoppingListItems;
using ListEntity = ShoppingList.API.Entities.ShoppingLists.ShoppingList;
using ListSearchObject = ShoppingList.API.Entities.ShoppingLists.ShoppingListSearchObject;

namespace ShoppingList.API.Infrastructure;

/// <summary>
/// Creates the SQLite database and seeds sample data (categories, ~500 articles, shoppers,
/// lists and list items) through the Regira <see cref="IEntityService{TEntity}"/> implementations,
/// so preppers, primers (timestamps) and normalizers (search content) all run as in production.
/// </summary>
public static class SeedData
{
    private const int ArticleCount = 500;

    private static readonly Dictionary<string, string[]> CategoryTree = new()
    {
        ["Fruit & Vegetables"] = ["Fresh Fruit", "Fresh Vegetables", "Salads", "Herbs", "Organic Produce"],
        ["Dairy & Eggs"] = ["Milk", "Cheese", "Yoghurt", "Butter & Margarine", "Eggs"],
        ["Meat & Fish"] = ["Beef", "Poultry", "Pork", "Fresh Fish", "Deli Meats"],
        ["Bakery"] = ["Bread", "Pastries", "Cakes", "Gluten Free"],
        ["Beverages"] = ["Water", "Soft Drinks", "Juices", "Coffee & Tea", "Beer & Wine"],
        ["Pantry"] = ["Pasta & Rice", "Canned Goods", "Sauces & Oils", "Spices", "Breakfast Cereals"],
        ["Frozen"] = ["Frozen Vegetables", "Ice Cream", "Frozen Meals", "Frozen Fish"],
        ["Snacks"] = ["Chips & Crisps", "Chocolate", "Biscuits", "Nuts & Seeds"],
        ["Household"] = ["Cleaning", "Laundry", "Kitchen Supplies", "Paper Goods"],
        ["Personal Care"] = ["Hygiene", "Hair Care", "Oral Care", "Baby Care"]
    };

    private static readonly string[] Units = ["piece", "kg", "g", "litre", "ml", "pack", "bottle", "can", "bunch"];

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var dbContext = sp.GetRequiredService<ShoppingDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        // Already seeded? Skip.
        if (await dbContext.Articles.AnyAsync())
            return;

        var categoryService = sp.GetRequiredService<IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes>>();
        var articleService = sp.GetRequiredService<IEntityService<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes>>();
        var shopperService = sp.GetRequiredService<IEntityService<Shopper>>();
        var listService = sp.GetRequiredService<IEntityService<ListEntity, int, ListSearchObject>>();
        var itemService = sp.GetRequiredService<IEntityService<ShoppingListItem, int, ShoppingListItemSearchObject>>();

        Randomizer.Seed = new Random(20260611);
        var faker = new Faker("en");

        var allCategories = await SeedCategoriesAsync(categoryService);
        await SeedArticlesAsync(articleService, allCategories, faker);
        var shoppers = await SeedShoppersAsync(shopperService, faker);
        await SeedListsAndItemsAsync(listService, itemService, dbContext, shoppers, faker);
    }

    private static async Task<List<Category>> SeedCategoriesAsync(
        IEntityService<Category, CategorySearchObject, EntitySortBy, CategoryIncludes> service)
    {
        // 1. Root categories first so their auto-increment Ids are available for the child links.
        var roots = new List<Category>();
        foreach (var rootName in CategoryTree.Keys)
        {
            var root = new Category { Title = rootName, Description = $"All {rootName.ToLowerInvariant()} products." };
            await service.Add(root);
            roots.Add(root);
        }
        await service.SaveChanges();

        // 2. Child categories, each linked to its parent through the self-referential join.
        var children = new List<Category>();
        foreach (var (root, childNames) in roots.Zip(CategoryTree.Values))
        {
            foreach (var childName in childNames)
            {
                var child = new Category
                {
                    Title = childName,
                    Description = $"{childName} in {root.Title}.",
                    ParentEntities = [new RelatedCategory { ParentId = root.Id }]
                };
                await service.Add(child);
                children.Add(child);
            }
        }
        await service.SaveChanges();

        return [.. roots, .. children];
    }

    private static async Task SeedArticlesAsync(
        IEntityService<Article, ArticleSearchObject, ArticleSortBy, EntityIncludes> service,
        List<Category> categories,
        Faker faker)
    {
        for (var i = 0; i < ArticleCount; i++)
        {
            // 1-3 distinct categories per article.
            var picks = faker.PickRandom(categories, faker.Random.Int(1, 3)).Distinct().ToList();

            var article = new Article
            {
                Title = faker.Commerce.ProductName(),
                Description = faker.Commerce.ProductDescription(),
                Brand = faker.Company.CompanyName(),
                Unit = faker.PickRandom(Units),
                Categories = [.. picks.Select(c => new ArticleCategory { CategoryId = c.Id })]
            };
            await service.Add(article);
        }
        await service.SaveChanges();
    }

    private static async Task<List<Shopper>> SeedShoppersAsync(IEntityService<Shopper> service, Faker faker)
    {
        var shoppers = new List<Shopper>();
        for (var i = 0; i < 12; i++)
        {
            var name = faker.Name.FullName();
            var shopper = new Shopper
            {
                Name = name,
                Email = faker.Internet.Email(name.Split(' ')[0], name.Split(' ').Last())
            };
            await service.Add(shopper);
            shoppers.Add(shopper);
        }
        await service.SaveChanges();
        return shoppers;
    }

    private static async Task SeedListsAndItemsAsync(
        IEntityService<ListEntity, int, ListSearchObject> listService,
        IEntityService<ShoppingListItem, int, ShoppingListItemSearchObject> itemService,
        ShoppingDbContext dbContext,
        List<Shopper> shoppers,
        Faker faker)
    {
        var listTemplates = new[] { "Weekly Groceries", "Weekend BBQ", "Party Supplies", "Quick Run", "Pantry Restock", "Healthy Week" };

        // 1. Lists first so their Ids are available for the items.
        var lists = new List<ListEntity>();
        foreach (var shopper in shoppers)
        {
            var listsForShopper = faker.Random.Int(1, 3);
            foreach (var name in faker.PickRandom(listTemplates, listsForShopper).Distinct())
            {
                var list = new ListEntity { Name = name, ShopperId = shopper.Id };
                await listService.Add(list);
                lists.Add(list);
            }
        }
        await listService.SaveChanges();

        // 2. Items: a random selection of articles per list, some active, some not.
        var articleIds = await dbContext.Articles.Select(a => a.Id).ToListAsync();
        foreach (var list in lists)
        {
            var pickedIds = faker.PickRandom(articleIds, faker.Random.Int(6, 15)).Distinct().ToList();
            foreach (var articleId in pickedIds)
            {
                var item = new ShoppingListItem
                {
                    ShoppingListId = list.Id,
                    ArticleId = articleId,
                    IsActive = faker.Random.Bool(0.7f),
                    Quantity = faker.Random.Int(1, 5),
                    Note = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence(3) : null
                };
                await itemService.Add(item);
            }
        }
        await itemService.SaveChanges();
    }
}
