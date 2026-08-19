using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Speakers;

public static class SpeakerServiceConfiguration
{
    // Simple registration — 2/5 simple budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddSpeakers(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Speaker>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
