using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Webshop.API.Data;

namespace Webshop.API.Entities.Customers;

public static class CustomerServiceConfiguration
{
    public static EntityServiceCollection<WebshopDbContext> AddCustomers(
        this IEntityServiceCollection<WebshopDbContext> services)
        => services.For<Customer, Guid>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Name));
            e.Prepare(item => item.Id = item.Id == Guid.Empty ? Guid.NewGuid() : item.Id);
        });
}
