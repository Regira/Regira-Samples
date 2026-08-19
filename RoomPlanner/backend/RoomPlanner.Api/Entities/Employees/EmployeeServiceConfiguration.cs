using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Employees;

public static class EmployeeServiceConfiguration
{
    // simple: 3/5 simple slots
    public static EntityServiceCollection<RoomPlannerDbContext> AddEmployees(this IEntityServiceCollection<RoomPlannerDbContext> services)
        => services.For<Employee, int, EmployeeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Department))
                {
                    query = query.Where(x => x.Department == so.Department);
                }
                if (so?.IsActive != null)
                {
                    query = query.Where(x => x.IsActive == so.IsActive);
                }
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
