using System.Text.RegularExpressions;
using Regira.Entities.Preppers.Abstractions;

namespace Blog.Api.Entities.Posts;

/// <summary>Derives ReadingTimeMinutes from Content and stamps PublishedAt on first publication.</summary>
public partial class BlogPostPrepper : EntityPrepperBase<BlogPost>
{
    private const int WordsPerMinute = 220;

    public override Task Prepare(BlogPost modified, BlogPost? original, CancellationToken token = default)
    {
        var words = string.IsNullOrWhiteSpace(modified.Content) ? 0 : WordRegex().Count(modified.Content);
        modified.ReadingTimeMinutes = words == 0 ? 0 : Math.Max(1, (int)Math.Round(words / (double)WordsPerMinute));

        // publishing without a date keeps the stored date, or means "now" on first publication
        // (a pre-set value - back-dated or scheduled - is kept)
        if (modified.IsPublished && modified.PublishedAt == null)
            modified.PublishedAt = original?.PublishedAt ?? DateTime.UtcNow;

        return Task.CompletedTask;
    }

    [GeneratedRegex(@"\b\w+\b")]
    private static partial Regex WordRegex();
}
