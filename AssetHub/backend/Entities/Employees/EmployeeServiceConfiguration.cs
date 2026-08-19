using AssetHub.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace AssetHub.Api.Entities.Employees;

// Budget: simple 5/5
public static class EmployeeServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddEmployees(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Employee, int, EmployeeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (!string.IsNullOrWhiteSpace(so?.Name))
                {
                    query = query.Where(x => x.FirstName.Contains(so.Name) || x.LastName.Contains(so.Name));
                }
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
            e.SortBy(query => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName));
        });
}
