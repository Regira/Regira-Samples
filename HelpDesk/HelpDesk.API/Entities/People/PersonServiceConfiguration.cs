using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;
using Regira.Entities.QueryBuilders.Abstractions;

namespace HelpDesk.API.Entities.People;

public class PersonQueryBuilder : FilteredQueryBuilderBase<Person, int, PersonSearchObject>
{
    public override IQueryable<Person> Build(IQueryable<Person> query, PersonSearchObject? so)
    {
        if (so == null) return query;
        if (so.Role?.Any() == true) query = query.Where(x => so.Role.Contains(x.Role));
        if (so.SupportTeamId?.Any() == true) query = query.Where(x => x.SupportTeamId != null && so.SupportTeamId.Contains(x.SupportTeamId.Value));
        if (so.IsActive != null) query = query.Where(x => x.IsActive == so.IsActive);
        return query;
    }
}

public static class PersonServiceConfiguration
{
    // 1/2 complex
    public static EntityServiceCollection<HelpDesk.API.Data.AppDbContext> AddPeople(this IEntityServiceCollection<HelpDesk.API.Data.AppDbContext> services)
        => services.For<Person, PersonSearchObject, PersonSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<PersonQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                PersonSortBy.Name => query.OrderOrThenBy(x => x.FullName),
                PersonSortBy.NameDesc => query.OrderOrThenByDescending(x => x.FullName),
                PersonSortBy.Created => query.OrderOrThenBy(x => x.Created),
                PersonSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.FullName)
            });
            // SupportTeam is a cheap to-one shown on every agent row - always loaded, no flag needed
            e.Includes((query, _) => query.Include(x => x.SupportTeam!));
            e.AddProcessor<PersonProcessor>();
        });
}
