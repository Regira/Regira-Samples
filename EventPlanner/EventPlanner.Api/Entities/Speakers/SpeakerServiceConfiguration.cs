using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Speakers;

public static class SpeakerServiceConfiguration
{
    public static EntityServiceCollection<EventPlannerDbContext> AddSpeakers(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Speaker, int, SpeakerSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Company?.Any() == true) query = query.Where(x => so.Company.Contains(x.Company!));
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName));
        });
}
