using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.EventCategories;
using EventPlanner.Api.Entities.Employees;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Sessions;
using EventPlanner.Api.Entities.Speakers;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;

namespace EventPlanner.Api.Extensions;

// Budget tally (free tier = 5 simple + 2 complex, two independent buckets):
//   Location          simple   1/5
//   Speaker           simple   2/5
//   EventCategory     simple   3/5
//   Employee          simple   4/5
//   Session           simple   5/5
//   Event             complex  1/2
//   Registration      complex  2/2
//   SessionSpeaker       owned via Related() on Session — no slot
//   RegistrationSession  owned via Related() on Registration — no slot
// -> 5 simple / 2 complex -> fits free tier exactly (7 registrations, the hard ceiling).
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<EventPlannerDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddLocations()
            .AddSpeakers()
            .AddEventCategories()
            .AddEmployees()
            .AddSessions()
            .AddEvents()
            .AddRegistrations();
}
