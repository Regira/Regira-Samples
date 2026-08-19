using EventPlanner.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace EventPlanner.Api.Entities.Employees;

public static class EmployeeServiceConfiguration
{
    // Simple registration — 4/5 simple budget slot
    public static EntityServiceCollection<EventPlannerDbContext> AddEmployees(this IEntityServiceCollection<EventPlannerDbContext> services)
        => services.For<Employee>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
