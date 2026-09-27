using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Employees;

public static class EmployeeServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddEmployees(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Employee, int, EmployeeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.Department?.Any() == true) query = query.Where(x => x.Department != null && so.Department.Contains(x.Department));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName));
            // Title is a derived display name (server-owned in practice: always recomputed here)
            e.Prepare(employee => employee.Title = $"{employee.FirstName} {employee.LastName}".Trim());
        });
}
