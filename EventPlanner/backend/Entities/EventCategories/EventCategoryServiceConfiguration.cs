using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.EventCategories;

public static class EventCategoryServiceConfiguration
{
    // Simple registration — 3/5 simple budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddEventCategories(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<EventCategory>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
