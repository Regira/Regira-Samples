using System.Text;
using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Posts;
using Blog.Api.Entities.Tags;
using Blog.Api.Utilities;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace Blog.Api.Data.Seeding;

/// <summary>
/// Seeds categories, tags and blog posts through the IEntityService implementations
/// (so preppers, normalizers and primers run exactly as they do for API writes).
/// </summary>
public class BlogSeeder(
    BlogDbContext dbContext,
    IEntityService<Category, int> categoryService,
    IEntityService<Tag, int> tagService,
    IEntityService<BlogPost, int> postService,
    ILogger<BlogSeeder> logger)
{
    private const int BatchSize = 100;

    public async Task Seed(int postCount, CancellationToken token = default)
    {
        if (await dbContext.BlogPosts.AnyAsync(token))
            return;

        Randomizer.Seed = new Random(20260927);
        var faker = new Faker("en");
        var now = DateTime.UtcNow;

        // --- wave 1: categories
        var sort = 0;
        foreach (var c in SeedCatalog.Categories)
        {
            await categoryService.Add(new Category
            {
                Title = c.Title,
                Description = c.Description,
                Color = c.Color,
                SortOrder = sort++ * 10,
                Created = now.AddYears(-3).AddDays(-30 + sort)
            });
        }
        await categoryService.SaveChanges(token);

        // --- wave 2: tags (category-specific + general)
        var tagTitles = SeedCatalog.Categories.SelectMany(c => c.Tags).Concat(SeedCatalog.GeneralTags).Distinct().ToList();
        foreach (var title in tagTitles)
            await tagService.Add(new Tag { Title = title, Created = now.AddYears(-3).AddDays(-20) });
        await tagService.SaveChanges(token);

        // reference earlier waves by key (they are detached after SaveChanges)
        var categoryIds = await dbContext.Categories.AsNoTracking().ToDictionaryAsync(x => x.Title, x => x.Id, token);
        var tagIds = await dbContext.Tags.AsNoTracking().ToDictionaryAsync(x => x.Title, x => x.Id, token);

        // --- wave 3: posts
        var authors = Enumerable.Range(0, 14).Select(_ => faker.Name.FullName()).ToArray();
        var usedTitles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var posts = new List<BlogPost>(postCount);

        while (posts.Count < postCount)
        {
            var cat = faker.PickRandom(SeedCatalog.Categories);
            var topic = faker.PickRandom(cat.Topics);
            var template = faker.PickRandom(SeedCatalog.TitleTemplates);
            var title = template
                .Replace("{T}", TitleCase(topic, capitalizeFirst: template.StartsWith("{T}")))
                .Replace("{n}", faker.Random.Int(3, 12).ToString());
            if (!usedTitles.Add(title))
                continue;

            // status mix: ~82% live, ~6% scheduled, ~12% draft
            var roll = faker.Random.Double();
            bool isPublished;
            DateTime? publishedAt;
            DateTime created;
            if (roll < 0.82)
            {
                isPublished = true;
                publishedAt = now.AddMinutes(-faker.Random.Int(60, 3 * 365 * 24 * 60));
                created = publishedAt.Value.AddHours(-faker.Random.Int(4, 14 * 24));
            }
            else if (roll < 0.88)
            {
                isPublished = true;
                publishedAt = now.AddHours(faker.Random.Int(12, 30 * 24));
                created = now.AddHours(-faker.Random.Int(2, 20 * 24));
            }
            else
            {
                isPublished = false;
                publishedAt = null;
                created = now.AddHours(-faker.Random.Int(2, 60 * 24));
            }
            // "Getting started ... in {Y}" names the year the post goes live
            title = title.Replace("{Y}", (publishedAt ?? now).Year.ToString());

            DateTime? lastModified = faker.Random.Bool(0.35f)
                ? created.AddHours(faker.Random.Double(1, Math.Max(2, (now - created).TotalHours - 1)))
                : null;

            // topic-specific tags when the topic has them, otherwise the category's pool
            var topicTags = SeedCatalog.TopicTags.TryGetValue(topic, out var tt) ? tt : cat.Tags;
            var tagSet = faker.PickRandom(topicTags, faker.Random.Int(1, Math.Min(2, topicTags.Length)))
                .Concat(faker.PickRandom(SeedCatalog.GeneralTags, faker.Random.Int(1, 2)))
                .Distinct()
                .ToList();

            var slug = SlugUtility.Slugify(title);
            posts.Add(new BlogPost
            {
                Title = title,
                Slug = slug,
                Summary = faker.PickRandom(SeedCatalog.SummaryTemplates).Replace("{t}", topic),
                Content = BuildContent(faker, topic),
                CoverImageUrl = $"https://picsum.photos/seed/{slug}/1200/675",
                AuthorName = faker.PickRandom(authors),
                CategoryId = categoryIds[cat.Title],
                IsPublished = isPublished,
                PublishedAt = publishedAt,
                IsFeatured = isPublished && publishedAt <= now && faker.Random.Bool(0.07f),
                Created = created,
                LastModified = lastModified,
                // owned join rows ride on the parent's navigation (no service of their own)
                Tags = tagSet.Select(t => new BlogPostTag { TagId = tagIds[t] }).ToList()
            });
        }

        foreach (var batch in posts.Chunk(BatchSize))
        {
            foreach (var post in batch)
                await postService.Add(post);
            await postService.SaveChanges(token);
        }

        logger.LogInformation("Seeded {Categories} categories, {Tags} tags and {Posts} blog posts",
            categoryIds.Count, tagIds.Count, posts.Count);
    }

    private static string BuildContent(Faker faker, string topic)
    {
        var sb = new StringBuilder();
        sb.AppendLine(Paragraph(faker, topic, 4, 6)).AppendLine();
        var headings = faker.PickRandom(SeedCatalog.Headings, faker.Random.Int(4, 6)).ToList();
        foreach (var heading in headings)
        {
            sb.AppendLine("## " + heading).AppendLine();
            var paragraphs = faker.Random.Int(2, 4);
            for (var i = 0; i < paragraphs; i++)
                sb.AppendLine(Paragraph(faker, topic, 3, 6)).AppendLine();
            if (heading == "A short checklist" || faker.Random.Bool(0.15f))
            {
                foreach (var item in faker.PickRandom(SeedCatalog.ListItems, faker.Random.Int(3, 5)))
                    sb.AppendLine("- " + item);
                sb.AppendLine();
            }
            else if (faker.Random.Bool(0.12f))
            {
                sb.AppendLine("> " + faker.PickRandom(SeedCatalog.Quotes)).AppendLine();
            }
        }
        return sb.ToString().TrimEnd();
    }

    private static string Paragraph(Faker faker, string topic, int min, int max)
        => string.Join(' ', faker.PickRandom(SeedCatalog.Sentences, faker.Random.Int(min, max))
            .Select(s => s.Replace("{T}", Capitalize(topic)).Replace("{t}", topic)));

    private static readonly HashSet<string> SmallWords = ["a", "an", "and", "by", "for", "in", "of", "the", "to", "on"];

    private static string TitleCase(string text, bool capitalizeFirst)
        => string.Join(' ', text.Split(' ').Select((w, i) => SmallWords.Contains(w) && (i > 0 || !capitalizeFirst) ? w : Capitalize(w)));

    private static string Capitalize(string w) => w.Length == 0 ? w : char.ToUpperInvariant(w[0]) + w[1..];
}
