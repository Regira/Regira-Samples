using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using RoomPlanner.Api.Data;

namespace RoomPlanner.Api.Entities.Equipments;

public static class EquipmentServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddEquipment(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Equipment>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });
}
