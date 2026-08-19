using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using HelpDesk.API.Data;

namespace HelpDesk.API.Entities.SupportTeams;

public static class SupportTeamServiceConfiguration
{
    // 1/5 simple
    public static EntityServiceCollection<AppDbContext> AddSupportTeams(this IEntityServiceCollection<AppDbContext> services)
        => services.For<SupportTeam>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            e.Includes((query, includes) => includes?.HasFlag(EntityIncludes.All) == true
                ? query.Include(x => x.Members!)
                : query);
            e.AddProcessor<SupportTeamProcessor>();
        });
}
