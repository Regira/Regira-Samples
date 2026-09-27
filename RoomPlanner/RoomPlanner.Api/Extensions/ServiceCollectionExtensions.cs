using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using RoomPlanner.Api.Data;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.Reservations;
using RoomPlanner.Api.Entities.Rooms;
using RoomPlanner.Api.Services;

namespace RoomPlanner.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Free-tier budget (5 simple + 2 complex):
    // | Entity              | Classification                                   | Tally        |
    // |---------------------|--------------------------------------------------|--------------|
    // | Building            | simple  For<Building>()                          | 1/5 simple   |
    // | Floor               | simple  For<Floor, int, FloorSearchObject>()     | 2/5 simple   |
    // | Equipment           | simple  For<Equipment>()                         | 3/5 simple   |
    // | Employee            | simple  For<Employee, int, EmployeeSearchObject> | 4/5 simple   |
    // | Room                | complex (RoomSortBy / RoomIncludes)              | 1/2 complex  |
    // | Reservation         | complex (ReservationSortBy / ReservationIncludes)| 2/2 complex  |
    // | RoomEquipment       | owned join via Room.Related()                    | -            |
    // | ReservationRoom     | owned join via Reservation.Related()             | -            |
    // | ReservationAttendee | owned join via Reservation.Related()             | -            |
    // => 4 simple / 2 complex -> fits the free tier
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.AddScoped<WorkflowContext>();
        return services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddBuildings()
            .AddFloors()
            .AddEquipment()
            .AddEmployees()
            .AddRooms()
            .AddReservations();
    }
}
