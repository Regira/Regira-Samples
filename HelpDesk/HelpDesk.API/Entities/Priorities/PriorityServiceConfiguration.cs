using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.Priorities;

public static class PriorityServiceConfiguration
{
    // 3/5 simple
    public static EntityServiceCollection<AppDbContext> AddPriorities(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Priority>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Level));
        });
}
