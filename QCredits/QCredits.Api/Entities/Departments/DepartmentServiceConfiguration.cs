using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;

namespace QCredits.Api.Entities.Departments;

public static class DepartmentServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddDepartments(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Department>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
