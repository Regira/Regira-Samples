using Regira.Entities.QueryBuilders.Abstractions;

namespace Blog.Api.Entities.Posts;

public class BlogPostQueryBuilder : FilteredQueryBuilderBase<BlogPost, int, BlogPostSearchObject>
{
    public override IQueryable<BlogPost> Build(IQueryable<BlogPost> query, BlogPostSearchObject? so)
    {
        if (so == null) return query;
        var now = DateTime.UtcNow;

        if (so.CategoryId?.Any() == true)
            query = query.Where(x => x.CategoryId != null && so.CategoryId.Contains(x.CategoryId.Value));
        if (!string.IsNullOrWhiteSpace(so.CategorySlug))
            query = query.Where(x => x.Category!.Slug == so.CategorySlug);
        if (so.TagId?.Any() == true)
            query = query.Where(x => x.Tags!.Any(t => so.TagId.Contains(t.TagId)));
        if (!string.IsNullOrWhiteSpace(so.TagSlug))
            query = query.Where(x => x.Tags!.Any(t => t.Tag!.Slug == so.TagSlug));
        if (!string.IsNullOrWhiteSpace(so.Slug))
            query = query.Where(x => x.Slug == so.Slug);
        if (!string.IsNullOrWhiteSpace(so.Author))
            query = query.Where(x => x.AuthorName == so.Author);

        if (so.IsPublished == true)
            query = query.Where(x => x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now);
        else if (so.IsPublished == false)
            query = query.Where(x => !x.IsPublished || x.PublishedAt == null || x.PublishedAt > now);

        query = so.Status switch
        {
            PostStatus.Draft => query.Where(x => !x.IsPublished),
            PostStatus.Scheduled => query.Where(x => x.IsPublished && x.PublishedAt > now),
            PostStatus.Published => query.Where(x => x.IsPublished && x.PublishedAt != null && x.PublishedAt <= now),
            _ => query
        };

        if (so.IsFeatured.HasValue)
            query = query.Where(x => x.IsFeatured == so.IsFeatured.Value);
        if (so.MinPublishedAt.HasValue)
            query = query.Where(x => x.PublishedAt >= so.MinPublishedAt.Value);
        if (so.MaxPublishedAt.HasValue)
            query = query.Where(x => x.PublishedAt <= so.MaxPublishedAt.Value);
        if (so.Year.HasValue)
        {
            var from = new DateTime(so.Year.Value, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var to = from.AddYears(1);
            query = query.Where(x => x.PublishedAt >= from && x.PublishedAt < to);
        }
        return query;
    }
}
