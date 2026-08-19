using Bogus;
using Regira.Entities.Services.Abstractions;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.MeetingRooms;
using RoomPlanner.Api.Entities.Reservations;

namespace RoomPlanner.Api.Data.Seeding;

/// <summary>
/// Seeds sample data for every entity via the registered IEntityService implementations (Bogus-generated).
/// Idempotent: skips entirely when buildings already exist.
/// </summary>
public static class SeedDataGenerator
{
    private static readonly string[] FloorNames =
    [
        "Ground Floor", "Floor 1", "Floor 2", "Floor 3", "Floor 4", "Floor 5"
    ];

    private static readonly string[] RoomNames =
    [
        "Everest", "K2", "Kilimanjaro", "Denali", "Matterhorn", "Fuji",
        "Orion", "Vega", "Sirius", "Polaris", "Andromeda", "Nebula",
        "Amber", "Cobalt", "Slate", "Ivory", "Onyx", "Coral",
        "Willow", "Maple", "Cedar", "Birch", "Aspen", "Juniper"
    ];

    private static readonly string[] Departments =
    [
        "Engineering", "Sales", "Marketing", "Finance", "Human Resources",
        "Customer Support", "Product", "Legal", "Operations", "IT"
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken token = default)
    {
        var buildingService = services.GetRequiredService<IEntityService<Building, int>>();
        if ((await buildingService.List(null, token: token)).Any())
        {
            return; // already seeded
        }

        Randomizer.Seed = new Random(20260819);
        var faker = new Faker();

        // ---- Buildings ----
        var buildingFaker = new Faker<Building>()
            .RuleFor(b => b.Title, f => f.Company.CompanyName() + " " + f.PickRandom("Campus", "Tower", "Center", "House", "Hub"))
            .RuleFor(b => b.Description, f => f.Lorem.Sentence(8))
            .RuleFor(b => b.Address, f => f.Address.StreetAddress())
            .RuleFor(b => b.City, f => f.Address.City());

        var buildings = buildingFaker.Generate(5);
        foreach (var building in buildings)
        {
            await buildingService.Add(building, token);
        }
        await buildingService.SaveChanges(token);

        // ---- Floors ----
        var floorService = services.GetRequiredService<IEntityService<Floor, int>>();
        var floors = new List<Floor>();
        foreach (var building in buildings)
        {
            var floorCount = faker.Random.Int(3, 5);
            for (var level = 0; level < floorCount; level++)
            {
                var floor = new Floor
                {
                    BuildingId = building.Id,
                    Title = FloorNames[Math.Min(level, FloorNames.Length - 1)],
                    Level = level
                };
                floors.Add(floor);
                await floorService.Add(floor, token);
            }
        }
        await floorService.SaveChanges(token);

        // ---- Meeting rooms ----
        var roomService = services.GetRequiredService<IEntityService<MeetingRoom, int>>();
        var rooms = new List<MeetingRoom>();
        var allEquipment = Enum.GetValues<RoomEquipment>().Where(e => e != RoomEquipment.None).ToArray();
        foreach (var floor in floors)
        {
            var roomCount = faker.Random.Int(3, 6);
            for (var i = 0; i < roomCount; i++)
            {
                var equipmentCount = faker.Random.Int(1, 4);
                var equipment = faker.PickRandom(allEquipment, equipmentCount)
                    .Aggregate(RoomEquipment.None, (acc, e) => acc | e);
                var capacity = faker.PickRandom(2, 4, 6, 8, 10, 12, 20, 30);
                var room = new MeetingRoom
                {
                    FloorId = floor.Id,
                    Title = faker.PickRandom(RoomNames) + " " + faker.Random.Int(1, 99),
                    Capacity = capacity,
                    Equipment = equipment,
                    RequiresApproval = capacity >= 12 || faker.Random.Bool(0.2f),
                    IsActive = faker.Random.Bool(0.95f)
                };
                rooms.Add(room);
                await roomService.Add(room, token);
            }
        }
        await roomService.SaveChanges(token);

        // ---- Employees ----
        var employeeService = services.GetRequiredService<IEntityService<Employee, int>>();
        var employeeFaker = new Faker<Employee>()
            .RuleFor(e => e.Title, f => f.Name.FullName())
            .RuleFor(e => e.Department, f => f.PickRandom(Departments))
            .RuleFor(e => e.JobTitle, f => f.Name.JobTitle())
            .RuleFor(e => e.IsActive, f => f.Random.Bool(0.97f));
        var employees = employeeFaker.Generate(150);
        var usedEmails = new HashSet<string>();
        foreach (var employee in employees)
        {
            string email;
            do
            {
                email = faker.Internet.Email(employee.Title.Split(' ')[0], employee.Title.Split(' ')[^1], "roomplanner-demo.local").ToLowerInvariant();
            } while (!usedEmails.Add(email));
            employee.Email = email;
            await employeeService.Add(employee, token);
        }
        await employeeService.SaveChanges(token);

        // ---- Reservations (primary entity, ~500) ----
        var reservationService = services.GetRequiredService<IEntityService<Reservation, int>>();
        var meetingTitles = new[]
        {
            "Sprint Planning", "Sync-up", "1:1", "Design Review", "Budget Review",
            "Client Call", "Onboarding", "All Hands", "Retro", "Kickoff",
            "Roadmap Discussion", "Interview", "Brainstorm", "Status Update", "Workshop",
            "Vendor Meeting", "Product Demo", "Strategy Session", "Training", "Town Hall"
        };
        var activeRooms = rooms.Where(r => r.IsActive).ToList();

        const int reservationCount = 500;
        for (var i = 0; i < reservationCount; i++)
        {
            var organizer = faker.PickRandom(employees);

            var dayOffset = faker.Random.Int(-30, 60);
            var date = DateTime.UtcNow.Date.AddDays(dayOffset);
            while (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                date = date.AddDays(1);
            }
            var startHour = faker.Random.Int(8, 17);
            var startMinute = faker.PickRandom(0, 15, 30, 45);
            var start = date.AddHours(startHour).AddMinutes(startMinute);
            var durationMinutes = faker.PickRandom(30, 30, 60, 60, 60, 90, 120);
            var end = start.AddMinutes(durationMinutes);

            var roomCount = faker.Random.Bool(0.85f) ? 1 : 2;
            var chosenRoomIds = faker.PickRandom(activeRooms, Math.Min(roomCount, activeRooms.Count))
                .Select(r => r.Id)
                .Distinct()
                .ToList();

            var reservation = new Reservation
            {
                Title = faker.PickRandom(meetingTitles),
                Description = faker.Random.Bool(0.5f) ? faker.Lorem.Sentence(10) : null,
                StartTime = start,
                EndTime = end,
                OrganizerId = organizer.Id,
                Rooms = chosenRoomIds.Select(id => new ReservationRoom { RoomId = id }).ToList()
            };

            var attendeeCount = faker.Random.Int(1, 6);
            var attendeePool = employees.Where(e => e.Id != organizer.Id).ToList();
            var attendeeEmployees = faker.PickRandom(attendeePool, Math.Min(attendeeCount, attendeePool.Count)).ToList();
            var attendees = attendeeEmployees.Select(a => new ReservationAttendee
            {
                EmployeeId = a.Id,
                ResponseStatus = faker.PickRandom<AttendeeResponseStatus>()
            }).ToList();
            if (faker.Random.Bool(0.15f))
            {
                attendees.Add(new ReservationAttendee
                {
                    ExternalName = faker.Name.FullName(),
                    ExternalEmail = faker.Internet.Email(),
                    ResponseStatus = faker.PickRandom<AttendeeResponseStatus>()
                });
            }
            reservation.Attendees = attendees;

            // 8% of reservations are seeded as already cancelled by the organizer.
            var precancel = faker.Random.Bool(0.08f);

            await reservationService.Add(reservation, token);
            if (precancel)
            {
                reservation.Status = ReservationStatus.Cancelled;
            }
        }
        await reservationService.SaveChanges(token);
    }
}
