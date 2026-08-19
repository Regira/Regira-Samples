using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using QCredits.Api.Data;

namespace QCredits.Api.Entities.Employees;

public static class EmployeeServiceConfiguration
{
    public static EntityServiceCollection<QCreditsDbContext> AddEmployees(this IEntityServiceCollection<QCreditsDbContext> services)
        => services.For<Employee, int, EmployeeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Department?.Any() == true)
                {
                    query = query.Where(x => x.Department != null && so.Department.Contains(x.Department));
                }
                if (so?.Role?.Any() == true)
                {
                    query = query.Where(x => so.Role.Contains(x.Role));
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
