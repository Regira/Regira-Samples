using Regira.Entities.QueryBuilders.Abstractions;

namespace Blog.Api.Entities.BlogPosts;

public class BlogPostQueryBuilder : FilteredQueryBuilderBase<BlogPost, int, BlogPostSearchObject>
{
    public override IQueryable<BlogPost> Build(IQueryable<BlogPost> query, BlogPostSearchObject? so)
    {
        if (so == null)
        {
            return query;
        }

        if (so.CategoryId?.Any() == true)
        {
            query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        }

        if (so.TagId?.Any() == true)
        {
            query = query.Where(x => x.Tags!.Any(t => so.TagId.Contains(t.TagId)));
        }

        if (so.IsPublished != null)
        {
            query = query.Where(x => x.IsPublished == so.IsPublished);
        }

        if (so.MinPublishedAt != null)
        {
            query = query.Where(x => x.PublishedAt >= so.MinPublishedAt);
        }

        if (so.MaxPublishedAt != null)
        {
            query = query.Where(x => x.PublishedAt <= so.MaxPublishedAt);
        }

        if (!string.IsNullOrWhiteSpace(so.Slug))
        {
            query = query.Where(x => x.Slug == so.Slug);
        }

        return query;
    }
}
