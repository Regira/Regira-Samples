using Blog.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Blog.Api.Entities.Tags;

public static class TagServiceConfiguration
{
    // simple registration - 1 simple slot
    public static EntityServiceCollection<BlogDbContext> AddTags(this IEntityServiceCollection<BlogDbContext> services)
        => services.For<Tag>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
