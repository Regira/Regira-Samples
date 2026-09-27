using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace QCredits.Api.Entities.Employees;

public static class EmployeeServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddEmployees(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Employee, int, EmployeeSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.DepartmentId?.Any() == true) query = query.Where(x => so.DepartmentId.Contains(x.DepartmentId));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
                if (!string.IsNullOrWhiteSpace(so.Email)) query = query.Where(x => x.Email == so.Email);
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName));
            // the department is shown on every list row -> unconditional to-one include
            e.Includes((query, _) => query.Include(x => x.Department));
            e.Prepare(item =>
            {
                item.Email = item.Email?.Trim().ToLowerInvariant();
            });
        });
}
