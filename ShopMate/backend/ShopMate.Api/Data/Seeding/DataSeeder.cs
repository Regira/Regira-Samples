using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;
using ShopMate.Api.Entities.Articles;
using ShopMate.Api.Entities.Categories;
using ShopMate.Api.Entities.Shoppers;
using ShopMate.Api.Entities.ShoppingLists;

namespace ShopMate.Api.Data.Seeding;

/// <summary>
/// Seeds every entity through its IEntityService (so preppers, normalizers and primers run):
/// categories (multi-parent DAG) -> shoppers -> shopping lists -> ~500 articles (the primary entity).
/// Idempotent: does nothing when shoppers already exist.
/// </summary>
public class DataSeeder(
    ShopMateDbContext db,
    IEntityService<Category, int> categoryService,
    IEntityService<Shopper, int> shopperService,
    IEntityService<ShoppingList, int> listService,
    IEntityService<Article, int> articleService,
    ILogger<DataSeeder> logger)
{
    public const int ArticleTarget = 500;

    public async Task Seed(CancellationToken token = default)
    {
        if (await db.Shoppers.AnyAsync(token))
            return;

        Randomizer.Seed = new Random(20260927);
        var faker = new Faker("en");
        var now = DateTime.UtcNow;

        var categoryIds = await SeedCategories(now);
        var shopperIds = await SeedShoppers(faker, now);
        var lists = await SeedLists(faker, shopperIds, now);
        var articleCount = await SeedArticles(faker, lists, categoryIds, now);

        logger.LogInformation("Seeded {Categories} categories, {Shoppers} shoppers, {Lists} lists, {Articles} articles",
            categoryIds.Count, shopperIds.Count, lists.Count, articleCount);
    }

    private async Task<Dictionary<string, int>> SeedCategories(DateTime now)
    {
        var ids = new Dictionary<string, int>();
        var pending = SeedCatalog.Categories.ToList();
        // topological waves: a category is added once all its parents have an id
        while (pending.Count > 0)
        {
            var wave = pending.Where(c => c.Parents.All(ids.ContainsKey)).ToList();
            if (wave.Count == 0)
                throw new InvalidOperationException("Category seed contains a cycle or an unknown parent");
            var entities = new List<(string Title, Category Entity)>();
            foreach (var seed in wave)
            {
                var entity = new Category
                {
                    Title = seed.Title,
                    Description = seed.Description,
                    Icon = seed.Icon,
                    Color = seed.Color,
                    Created = now.AddDays(-120),
                    // hierarchy links travel on the owning navigation (join rows have no service)
                    ParentEntities = seed.Parents.Select(p => new RelatedCategory { ParentId = ids[p] }).ToList()
                };
                await categoryService.Add(entity);
                entities.Add((seed.Title, entity));
            }
            await categoryService.SaveChanges();
            foreach (var (title, entity) in entities)
                ids[title] = entity.Id;
            pending = pending.Except(wave).ToList();
        }
        return ids;
    }

    private async Task<List<int>> SeedShoppers(Faker faker, DateTime now)
    {
        string[] colors = ["#e53935", "#8e24aa", "#3949ab", "#039be5", "#00897b", "#7cb342", "#fb8c00", "#6d4c41"];
        var shoppers = new List<Shopper>();
        for (var i = 0; i < 8; i++)
        {
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var shopper = new Shopper
            {
                Name = $"{first} {last}",
                Email = faker.Internet.Email(first, last).ToLowerInvariant(),
                Color = colors[i % colors.Length],
                Created = now.AddDays(-faker.Random.Int(60, 90))
            };
            await shopperService.Add(shopper);
            shoppers.Add(shopper);
        }
        await shopperService.SaveChanges();
        return shoppers.Select(x => x.Id).ToList();
    }

    private async Task<List<(ShoppingList List, SeedCatalog.ListSeed Seed)>> SeedLists(Faker faker, List<int> shopperIds, DateTime now)
    {
        var result = new List<(ShoppingList, SeedCatalog.ListSeed)>();
        foreach (var shopperId in shopperIds)
        {
            // every shopper gets the weekly list plus 2-4 themed ones
            var seeds = new[] { SeedCatalog.Lists[0] }
                .Concat(faker.PickRandom(SeedCatalog.Lists.Skip(1), faker.Random.Int(2, 4)))
                .ToList();
            foreach (var seed in seeds)
            {
                var list = new ShoppingList
                {
                    ShopperId = shopperId,   // reference earlier waves by FK, never by navigation
                    Title = seed.Title,
                    Description = seed.Description,
                    Color = seed.Color,
                    IsPinned = seed == SeedCatalog.Lists[0],
                    Created = now.AddDays(-faker.Random.Int(5, 55))
                };
                await listService.Add(list);
                result.Add((list, seed));
            }
        }
        await listService.SaveChanges();
        return result;
    }

    private async Task<int> SeedArticles(Faker faker, List<(ShoppingList List, SeedCatalog.ListSeed Seed)> lists,
        Dictionary<string, int> categoryIds, DateTime now)
    {
        // spread ArticleTarget over the lists, weighted towards the weekly lists
        var weights = lists.Select(l => l.List.IsPinned ? 2.0 : 1.0).ToList();
        var totalWeight = weights.Sum();
        var perList = weights.Select(w => (int)Math.Round(ArticleTarget * w / totalWeight)).ToList();
        perList[0] += ArticleTarget - perList.Sum();

        var total = 0;
        for (var i = 0; i < lists.Count; i++)
        {
            var (list, seed) = lists[i];
            var candidates = seed.FocusCategories.Length == 0
                ? SeedCatalog.Products
                : SeedCatalog.Products.Where(p => p.Categories.Any(c => InFocus(c, seed.FocusCategories))).ToArray();
            // pad with the whole catalog when a themed list's pool is too small
            var pool = faker.Random.Shuffle(candidates).ToList();
            if (pool.Count < perList[i])
                pool.AddRange(faker.Random.Shuffle(SeedCatalog.Products.Except(candidates)));
            var products = pool.Take(perList[i]).ToList();

            var sortOrder = 0;
            foreach (var product in products)
            {
                var created = list.Created.AddHours(faker.Random.Double(1, Math.Max(2, (now - list.Created).TotalHours - 1)));
                var article = new Article
                {
                    ShoppingListId = list.Id,
                    Title = product.Title,
                    Quantity = faker.PickRandom(product.Quantities),
                    Unit = product.Unit,
                    Description = faker.Random.Bool(0.2f) ? faker.PickRandom(SeedCatalog.NotesFor(product)) : null,
                    // ~60 % still to buy; bought items are the older ones on the list
                    IsActive = faker.Random.Bool(0.6f),
                    SortOrder = ++sortOrder,
                    Created = created,
                    Categories = product.Categories
                        .Select(c => new ArticleCategory { CategoryId = categoryIds[c] })
                        .ToList()
                };
                await articleService.Add(article);
                total++;
            }
            await articleService.SaveChanges();
        }
        return total;
    }

    private static bool InFocus(string category, string[] focus)
    {
        if (focus.Contains(category)) return true;
        var seed = SeedCatalog.Categories.First(c => c.Title == category);
        return seed.Parents.Any(p => InFocus(p, focus));
    }
}
