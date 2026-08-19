using Blog.Api.Data;
using Blog.Api.Entities.BlogPosts;
using Blog.Api.Entities.Categories;
using Blog.Api.Entities.Tags;
using Bogus;
using Regira.Entities.Services.Abstractions;

namespace Blog.Api.Infrastructure;

public static class SeedData
{
    private static readonly string[] CategoryNames =
    [
        "Technology", "Travel", "Food & Cooking", "Health & Wellness", "Business",
        "Lifestyle", "Science", "Culture & Arts", "Sports", "Personal Finance"
    ];

    private static readonly string[] TagNames =
    [
        "tutorial", "news", "review", "opinion", "guide", "interview", "case-study",
        "trends", "how-to", "deep-dive", "beginner", "advanced", "product", "startup",
        "remote-work", "sustainability", "artificial-intelligence", "design",
        "productivity", "travel-tips", "recipe", "fitness", "mental-health", "budgeting"
    ];

    public static async Task SeedAsync(BlogDbContext dbContext, IServiceProvider services, CancellationToken token = default)
    {
        if (dbContext.Categories.Any())
        {
            return; // already seeded
        }

        var categoryService = services.GetRequiredService<IEntityService<Category, int>>();
        var tagService = services.GetRequiredService<IEntityService<Tag, int>>();
        var blogPostService = services.GetRequiredService<IEntityService<BlogPost, int>>();

        var slugs = new HashSet<string>();

        // --- Categories ---
        var categoryFaker = new Faker("en");
        var categories = CategoryNames.Select(name => new Category
        {
            Title = name,
            Slug = ToSlug(name, slugs),
            Description = categoryFaker.Lorem.Sentence(12)
        }).ToList();

        foreach (var category in categories)
        {
            await categoryService.Add(category, token: token);
        }
        await categoryService.SaveChanges(token);

        // --- Tags ---
        var tags = TagNames.Select(name => new Tag
        {
            Title = ToTitleCase(name),
            Slug = ToSlug(name, slugs)
        }).ToList();

        foreach (var tag in tags)
        {
            await tagService.Add(tag, token: token);
        }
        await tagService.SaveChanges(token);

        // --- Blog posts ---
        var random = new Random(20260819);
        var titleFaker = new Faker("en");
        const int postCount = 520;
        var batch = new List<BlogPost>(postCount);

        for (var i = 0; i < postCount; i++)
        {
            var title = ToTitleCase(titleFaker.Lorem.Sentence(random.Next(4, 9)).TrimEnd('.'));
            var slug = ToSlug(title, slugs);
            var category = categories[random.Next(categories.Count)];

            var isPublished = random.NextDouble() < 0.82;
            DateTime? publishedAt = null;
            var created = DateTime.UtcNow.AddDays(-random.Next(1, 730)).AddHours(-random.Next(0, 24));

            if (isPublished)
            {
                publishedAt = created.AddHours(random.Next(0, 6));
            }
            else if (random.NextDouble() < 0.3)
            {
                // a few scheduled (future) posts
                publishedAt = DateTime.UtcNow.AddDays(random.Next(1, 30));
            }

            var paragraphCount = random.Next(4, 9);
            var content = string.Join("\n\n", titleFaker.Lorem.Paragraphs(paragraphCount, "\n\n").Split("\n\n"));

            var tagCount = random.Next(1, 5);
            var postTags = tags
                .OrderBy(_ => random.Next())
                .Take(tagCount)
                .Select(t => new BlogPostTag { TagId = t.Id })
                .ToList();

            var post = new BlogPost
            {
                Title = title,
                Slug = slug,
                Summary = titleFaker.Lorem.Sentence(random.Next(14, 24)),
                Content = content,
                CoverImageUrl = $"https://picsum.photos/seed/{slug}/900/540",
                IsPublished = isPublished,
                PublishedAt = publishedAt,
                CategoryId = category.Id,
                Tags = postTags,
                Created = created
            };

            batch.Add(post);
        }

        foreach (var post in batch)
        {
            await blogPostService.Add(post, token: token);
        }
        await blogPostService.SaveChanges(token);
    }

    private static string ToSlug(string value, HashSet<string> usedSlugs)
    {
        var lowered = value.ToLowerInvariant();
        var chars = lowered.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray();
        var raw = new string(chars);
        while (raw.Contains("--"))
        {
            raw = raw.Replace("--", "-");
        }
        raw = raw.Trim('-');
        if (raw.Length > 80)
        {
            raw = raw[..80].Trim('-');
        }
        if (raw.Length == 0)
        {
            raw = "post";
        }

        var slug = raw;
        var suffix = 2;
        while (!usedSlugs.Add(slug))
        {
            slug = $"{raw}-{suffix}";
            suffix++;
        }

        return slug;
    }

    private static string ToTitleCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < words.Length; i++)
        {
            var word = words[i];
            words[i] = word.Length > 1
                ? char.ToUpperInvariant(word[0]) + word[1..]
                : word.ToUpperInvariant();
        }

        return string.Join(' ', words);
    }
}
