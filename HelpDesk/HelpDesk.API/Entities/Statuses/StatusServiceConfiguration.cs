using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.Statuses;

public static class StatusServiceConfiguration
{
    // 4/5 simple
    public static EntityServiceCollection<AppDbContext> AddStatuses(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Status>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.SortOrder));
        });
}
