using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;

namespace HelpDesk.Api.Entities.Persons;

public static class PersonServiceConfiguration
{
    public static EntityServiceCollection<HelpDeskDbContext> AddPersons(this IEntityServiceCollection<HelpDeskDbContext> services)
        => services.For<Person, PersonSearchObject, PersonSortBy, EntityIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Role?.Any() == true) query = query.Where(x => so.Role.Contains(x.Role));
                if (so.SupportTeamId?.Any() == true) query = query.Where(x => x.SupportTeamId != null && so.SupportTeamId.Contains(x.SupportTeamId.Value));
                if (so.HasAccount.HasValue) query = so.HasAccount.Value ? query.Where(x => x.UserId != null) : query.Where(x => x.UserId == null);
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                PersonSortBy.NameDesc => query.OrderOrThenByDescending(x => x.FamilyName).ThenByDescending(x => x.GivenName),
                PersonSortBy.Company => query.OrderOrThenBy(x => x.Company).ThenBy(x => x.FamilyName),
                PersonSortBy.Newest => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.FamilyName).ThenBy(x => x.GivenName)
            });
            e.Includes((query, _) => query.Include(x => x.SupportTeam));
        });
}
