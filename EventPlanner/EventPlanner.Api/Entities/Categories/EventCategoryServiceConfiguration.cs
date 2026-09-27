using EventPlanner.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Categories;

public static class EventCategoryServiceConfiguration
{
    public static EntityServiceCollection<EventPlannerDbContext> AddEventCategories(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<EventCategory>(e =>
        {
            // EventCategory has no NormalizedContent, so the global ?q= filter does not apply: search the title here
            e.Filter((query, so) =>
            {
                // the SPA sends wildcard terms (*con* *fer*): every term must match the title
                foreach (var term in (so?.Q ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim('*')).Where(t => t.Length > 0))
                {
                    var pattern = $"%{term}%";
                    query = query.Where(x => EF.Functions.Like(x.Title, pattern));
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
