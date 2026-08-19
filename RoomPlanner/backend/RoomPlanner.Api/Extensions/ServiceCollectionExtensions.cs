using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using RoomPlanner.Api.Data;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.MeetingRooms;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Budget tally (free tier = 5 simple + 2 complex):
    //   Building        simple   1/5
    //   Floor           simple   2/5
    //   Employee        simple   3/5
    //   MeetingRoom     complex  1/2
    //   Reservation     complex  2/2
    //   ReservationRoom       owned via e.Related() on Reservation - no slot
    //   ReservationAttendee   owned via e.Related() on Reservation - no slot
    // -> 3 simple / 2 complex, fits the free tier.
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<RoomPlannerDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddBuildings()
            .AddFloors()
            .AddEmployees()
            .AddMeetingRooms()
            .AddReservations();
}
