using HelpDesk.Api.Data;
using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace HelpDesk.Api.Entities;

/// <summary>The four admin-managed lookups — simple registrations with one fixed order each.</summary>
public static class ReferenceDataServiceConfiguration
{
    public static EntityServiceCollection<HelpDeskDbContext> AddSupportTeams(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<SupportTeam>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
            // Members is a collection: gated, so it loads on Details only
            e.Includes((query, includes) => includes?.HasFlag(EntityIncludes.All) == true
                ? query.Include(x => x.Members!.OrderBy(m => m.FamilyName))
                : query);
        });

    public static EntityServiceCollection<HelpDeskDbContext> AddCategories(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.SupportTeamId?.Any() == true) query = query.Where(x => x.SupportTeamId != null && so.SupportTeamId.Contains(x.SupportTeamId.Value));
                if (so?.IsActive != null) query = query.Where(x => x.IsActive == so.IsActive);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
            // to-one shown on every list row: eager-loaded unconditionally
            e.Includes((query, _) => query.Include(x => x.SupportTeam));
        });

    public static EntityServiceCollection<HelpDeskDbContext> AddPriorities(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<Priority>(e => e.SortBy(query => query.OrderByDescending(x => x.Level)));

    public static EntityServiceCollection<HelpDeskDbContext> AddStatuses(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<Status>(e => e.SortBy(query => query.OrderBy(x => x.SortOrder)));
}
