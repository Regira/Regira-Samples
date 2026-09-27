using Fleet.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace Fleet.Api.Entities.InterventionTypes;

public static class InterventionTypeServiceConfiguration
{
    public static EntityServiceCollection<FleetDbContext> AddInterventionTypes(this IEntityServiceCollection<FleetDbContext> services)
        => services.For<InterventionType, int, InterventionTypeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Category?.Any() == true) query = query.Where(x => so.Category.Contains(x.Category));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Category).ThenBy(x => x.Title));
            e.SetPageSize(defaultPageSize: 50, maxPageSize: 200);
        });
}
