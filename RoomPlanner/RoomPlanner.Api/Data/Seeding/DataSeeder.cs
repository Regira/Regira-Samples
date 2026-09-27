using Bogus;
using Regira.Entities.Services.Abstractions;
using RoomPlanner.Api.Entities.Buildings;
using RoomPlanner.Api.Entities.Employees;
using RoomPlanner.Api.Entities.Equipments;
using RoomPlanner.Api.Entities.Floors;
using RoomPlanner.Api.Entities.Reservations;
using RoomPlanner.Api.Entities.Rooms;
using RoomPlanner.Api.Services;

namespace RoomPlanner.Api.Data.Seeding;

/// <summary>
/// Seeds every entity through its IEntityService (so preppers, primers and normalizers run).
/// Waves: equipment -> buildings -> floors -> rooms -> employees -> reservations (~500, the primary entity).
/// The seeder is a trusted writer: it stamps historical approval / cancel states itself.
/// </summary>
public class DataSeeder(
    IEntityService<Equipment, int> equipmentService,
    IEntityService<Building, int> buildingService,
    IEntityService<Floor, int> floorService,
    IEntityService<Room, int> roomService,
    IEntityService<Employee, int> employeeService,
    IEntityService<Reservation, int> reservationService,
    WorkflowContext workflow,
    ILogger<DataSeeder> logger)
{
    const string Domain = "roomplanner.example";
    readonly Random _rnd = new(20260927);

    public async Task Seed(int reservationCount, CancellationToken token = default)
    {
        Randomizer.Seed = new Random(20260927);
        workflow.IsTrustedWriter = true;

        var equipment = await SeedEquipment(token);
        var buildings = await SeedBuildings(token);
        var floors = await SeedFloors(buildings, token);
        var rooms = await SeedRooms(buildings, floors, equipment, token);
        var employees = await SeedEmployees(token);
        var count = await SeedReservations(reservationCount, rooms, floors, employees, token);

        logger.LogInformation("Seeded {Equipment} equipment types, {Buildings} buildings, {Floors} floors, {Rooms} rooms, {Employees} employees, {Reservations} reservations",
            equipment.Count, buildings.Count, floors.Count, rooms.Count, employees.Count, count);
    }

    // ---------------------------------------------------------------- equipment
    async Task<Dictionary<string, Equipment>> SeedEquipment(CancellationToken token)
    {
        (string Code, string Title, string Icon, string Description)[] items =
        [
            ("PROJ", "Projector", "projector", "Ceiling-mounted HD projector with HDMI and USB-C input."),
            ("WB", "Whiteboard", "easel", "Wall whiteboard with markers and eraser."),
            ("VC", "Video conferencing", "camera-video", "Teams/Zoom room system with PTZ camera and ceiling microphones."),
            ("TV", "Display screen", "tv", "65\" or larger display with wireless casting."),
            ("SPK", "Speakerphone", "telephone", "Conference speakerphone for audio calls."),
            ("FLIP", "Flipchart", "journal", "Flipchart stand with paper pads."),
            ("MIC", "Microphones & PA", "mic", "Handheld/lapel microphones with PA system."),
            ("ACC", "Wheelchair accessible", "universal-access", "Step-free access and adjustable-height table."),
            ("STAND", "Standing table", "arrows-vertical", "High table for short stand-up meetings."),
        ];
        var result = new Dictionary<string, Equipment>();
        foreach (var (code, title, icon, description) in items)
        {
            var item = new Equipment { Code = code, Title = title, Icon = icon, Description = description, Created = DateTime.UtcNow.AddDays(-400) };
            await equipmentService.Add(item, token);
            result[code] = item;
        }
        await equipmentService.SaveChanges(token);
        return result;
    }

    // ---------------------------------------------------------------- buildings & floors
    record BuildingSeed(string Code, string Title, string Address, string City, int Floors, string[] RoomNames, string Theme);

    static readonly BuildingSeed[] BuildingSeeds =
    [
        new("HQ", "Atrium Tower", "Rue de la Loi 42", "Brussels", 6,
            ["Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto", "Ceres", "Eris", "Titan", "Europa", "Io", "Callisto", "Ganymede", "Phobos", "Deimos", "Triton", "Luna", "Vesta", "Pallas"], "planets"),
        new("ANT", "Harbour House", "Kattendijkdok-Westkaai 18", "Antwerp", 4,
            ["Scheldt", "Meuse", "Rhine", "Danube", "Seine", "Thames", "Elbe", "Loire", "Tagus", "Po", "Oder", "Vistula", "Ebro", "Rhone", "Dnieper", "Volga"], "rivers"),
        new("GNT", "Campus Gent", "Coupure Links 653", "Ghent", 3,
            ["Van Eyck", "Rubens", "Magritte", "Ensor", "Bruegel", "Memling", "Delvaux", "Permeke", "Van Dyck", "Jordaens", "Khnopff", "Wouters"], "painters"),
        new("LEU", "Innovation Lab", "Kapeldreef 75", "Leuven", 3,
            ["Lemaitre", "Mercator", "Vesalius", "Solvay", "Baekeland", "Sax", "Horta", "Stevin", "Dodoens", "Quetelet", "Plateau", "Gramme"], "scientists"),
    ];

    async Task<List<Building>> SeedBuildings(CancellationToken token)
    {
        var result = new List<Building>();
        foreach (var seed in BuildingSeeds)
        {
            var building = new Building
            {
                Code = seed.Code,
                Title = seed.Title,
                Address = seed.Address,
                City = seed.City,
                Description = $"{seed.City} office - meeting rooms are named after {seed.Theme}.",
                OpensAt = new TimeOnly(7, 0),
                ClosesAt = seed.Code == "LEU" ? new TimeOnly(21, 0) : new TimeOnly(19, 0),
                Created = DateTime.UtcNow.AddDays(-400),
            };
            await buildingService.Add(building, token);
            result.Add(building);
        }
        await buildingService.SaveChanges(token);
        return result;
    }

    async Task<List<Floor>> SeedFloors(List<Building> buildings, CancellationToken token)
    {
        var result = new List<Floor>();
        foreach (var building in buildings)
        {
            var seed = BuildingSeeds.First(x => x.Code == building.Code);
            for (var level = 0; level < seed.Floors; level++)
            {
                var floor = new Floor
                {
                    BuildingId = building.Id,
                    Level = level,
                    Title = level == 0 ? "Ground floor" : $"Floor {level}",
                    Created = DateTime.UtcNow.AddDays(-400),
                };
                await floorService.Add(floor, token);
                result.Add(floor);
            }
        }
        await floorService.SaveChanges(token);
        return result;
    }

    // ---------------------------------------------------------------- rooms
    // size profiles drawn as correlated tuples: capacity range, equipment set, approval rule
    record RoomProfile(string Kind, int MinCap, int MaxCap, string[] Equipment, string[] Optional, double ApprovalChance);

    static readonly RoomProfile Booth = new("Focus booth", 2, 3, ["TV"], ["STAND"], 0);
    static readonly RoomProfile Small = new("Huddle room", 4, 6, ["TV", "WB"], ["VC", "SPK", "STAND"], 0);
    static readonly RoomProfile Medium = new("Meeting room", 8, 12, ["TV", "WB", "VC"], ["SPK", "FLIP", "ACC"], 0.1);
    static readonly RoomProfile Large = new("Conference room", 14, 20, ["PROJ", "WB", "VC", "SPK"], ["FLIP", "ACC", "TV"], 0.4);
    static readonly RoomProfile Board = new("Boardroom", 16, 24, ["TV", "VC", "SPK"], ["ACC", "WB"], 1.0);
    static readonly RoomProfile Hall = new("Auditorium", 60, 120, ["PROJ", "MIC", "VC", "ACC"], ["TV"], 1.0);

    static readonly string[] Colors = ["#4e79a7", "#f28e2b", "#e15759", "#76b7b2", "#59a14f", "#edc948", "#b07aa1", "#ff9da7", "#9c755f", "#bab0ac"];

    async Task<List<Room>> SeedRooms(List<Building> buildings, List<Floor> floors, Dictionary<string, Equipment> equipment, CancellationToken token)
    {
        var result = new List<Room>();
        var colorIndex = 0;
        foreach (var building in buildings)
        {
            var seed = BuildingSeeds.First(x => x.Code == building.Code);
            var names = new Queue<string>(seed.RoomNames);
            var buildingFloors = floors.Where(f => f.BuildingId == building.Id).OrderBy(f => f.Level).ToList();
            foreach (var floor in buildingFloors)
            {
                // ground floor hosts the big rooms, the top floor the boardroom
                var profiles = new List<RoomProfile>();
                if (floor.Level == 0) profiles.AddRange(seed.Code is "HQ" or "GNT" ? [Hall, Large, Medium] : [Large, Medium, Small]);
                else if (floor == buildingFloors[^1]) profiles.AddRange([Board, Medium, Small, Booth]);
                else profiles.AddRange([Medium, Small, Small, Booth]);

                foreach (var profile in profiles)
                {
                    if (names.Count == 0) break;
                    var name = names.Dequeue();
                    var capacity = _rnd.Next(profile.MinCap, profile.MaxCap + 1);
                    if (profile == Hall) capacity = (int)Math.Round(capacity / 10.0) * 10;
                    var codes = profile.Equipment.Concat(profile.Optional.Where(_ => _rnd.NextDouble() < 0.4)).Distinct().ToList();
                    var room = new Room
                    {
                        Title = name,
                        Code = $"{building.Code}-{floor.Level}{result.Count(r => r.FloorId == floor.Id) + 1:00}",
                        Description = $"{profile.Kind} on {floor.Title.ToLowerInvariant()} of {building.Title}.",
                        FloorId = floor.Id,
                        Capacity = capacity,
                        RequiresApproval = _rnd.NextDouble() < profile.ApprovalChance,
                        IsActive = true,
                        Color = Colors[colorIndex++ % Colors.Length],
                        Created = DateTime.UtcNow.AddDays(-_rnd.Next(200, 400)),
                        Equipment = codes.Select(c => new RoomEquipment
                        {
                            EquipmentId = equipment[c].Id,
                            Quantity = c == "TV" && capacity >= 14 ? 2 : c == "MIC" ? 4 : 1,
                        }).ToList(),
                    };
                    await roomService.Add(room, token);
                    result.Add(room);
                }
            }
        }
        // a couple of rooms are out of service (renovation)
        foreach (var room in result.Where(r => r.Capacity is >= 4 and <= 12).OrderBy(_ => _rnd.Next()).Take(2))
        {
            room.IsActive = false;
            room.Description += " Temporarily out of service (renovation).";
        }
        await roomService.SaveChanges(token);
        return result;
    }

    // ---------------------------------------------------------------- employees
    static readonly (string Department, string[] JobTitles)[] Departments =
    [
        ("Engineering", ["Software Engineer", "Senior Software Engineer", "Tech Lead", "Engineering Manager", "QA Engineer", "DevOps Engineer"]),
        ("Product", ["Product Manager", "Product Owner", "UX Designer", "UX Researcher"]),
        ("Sales", ["Account Executive", "Sales Manager", "Sales Development Rep", "Key Account Manager"]),
        ("Marketing", ["Marketing Manager", "Content Strategist", "Brand Designer", "Growth Marketer"]),
        ("Finance", ["Controller", "Accountant", "Financial Analyst", "CFO"]),
        ("HR", ["HR Business Partner", "Recruiter", "Talent Manager", "HR Director"]),
        ("Operations", ["Facility Manager", "Office Manager", "Operations Analyst", "Procurement Officer"]),
        ("Customer Success", ["Customer Success Manager", "Support Engineer", "Support Team Lead"]),
        ("Legal", ["Legal Counsel", "Compliance Officer"]),
    ];

    async Task<List<Employee>> SeedEmployees(CancellationToken token)
    {
        var faker = new Faker("nl_BE");
        var emails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<Employee>();
        var weights = new[] { 30, 10, 18, 10, 8, 6, 8, 12, 3 };
        while (result.Count < 150)
        {
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var email = $"{Slug(first)}.{Slug(last)}@{Domain}";
            if (!emails.Add(email)) continue;
            var dept = Departments[WeightedIndex(weights)];
            var employee = new Employee
            {
                FirstName = first,
                LastName = last,
                Email = email,
                Department = dept.Department,
                JobTitle = faker.PickRandom(dept.JobTitles),
                Phone = $"+32 {_rnd.Next(470, 500)} {_rnd.Next(10, 99)} {_rnd.Next(10, 99)} {_rnd.Next(10, 99)}",
                IsActive = _rnd.NextDouble() > 0.04,
                Created = DateTime.UtcNow.AddDays(-_rnd.Next(30, 900)),
            };
            await employeeService.Add(employee, token);
            result.Add(employee);
        }
        await employeeService.SaveChanges(token);
        return result;
    }

    // ---------------------------------------------------------------- reservations
    // meeting types as correlated tuples: typical size, duration (minutes), share
    record MeetingType(string[] Titles, int MinPeople, int MaxPeople, int[] Durations, int Weight, bool MultiRoom = false);

    static readonly MeetingType[] MeetingTypes =
    [
        new(["1:1 {0}", "Coaching session", "Performance check-in", "Mentoring"], 2, 2, [30, 45, 60], 22),
        new(["Daily stand-up {0}", "Sync {0}", "Quick huddle"], 3, 6, [15, 30], 14),
        new(["Sprint planning", "Backlog refinement", "Retrospective", "Design review", "Budget review", "Pipeline review", "Campaign kick-off", "Customer escalation", "Hiring panel"], 4, 10, [60, 90, 120], 34),
        new(["Customer demo", "Vendor pitch", "Partner meeting", "Interview - {0}"], 3, 8, [45, 60, 90], 12),
        new(["{0} workshop", "Quarterly business review", "Roadmap workshop", "Training: {0}"], 8, 18, [120, 180, 240], 10),
        new(["Board meeting", "Management team", "Leadership offsite prep"], 6, 16, [90, 120, 180], 4),
        new(["All-hands (streamed)", "Town hall", "Company update", "Onboarding day"], 25, 110, [60, 90, 120], 4, MultiRoom: true),
    ];

    static readonly string[] Agendas =
    [
        "Agenda: 1) status update 2) blockers 3) next steps.",
        "Please review the {0} notes before the meeting.",
        "Dial-in details are in the Teams invite for remote colleagues.",
        "Bring your laptop - we'll work through the {0} backlog together.",
        "Decision needed on {0}; figures will be shared on screen.",
        "Coffee and sandwiches ordered via Operations.",
        "Follow-up of last week's session on {0}.",
        "Short sync - please be on time.",
    ];

    static readonly string[] Topics = ["Data platform", "Pricing", "Security awareness", "GDPR", "Onboarding", "Mobile app", "Q4 targets", "Hybrid work", "Customer journey", "Cloud costs", "Accessibility", "Brand refresh"];

    async Task<int> SeedReservations(int count, List<Room> rooms, List<Floor> floors, List<Employee> employees, CancellationToken token)
    {
        var faker = new Faker("en");
        var tz = TimeZoneInfo.Local;
        var todayLocal = DateTime.Now.Date;
        var nowUtc = DateTime.UtcNow;
        var floorBuilding = floors.ToDictionary(f => f.Id, f => f.BuildingId);
        var roomsByBuilding = rooms.GroupBy(r => floorBuilding[r.FloorId]).ToDictionary(g => g.Key, g => g.ToList());
        var buildingIds = roomsByBuilding.Keys.ToList();
        var buildingWeights = buildingIds.Select(id => roomsByBuilding[id].Count).ToArray();
        var occupancy = rooms.ToDictionary(r => r.Id, _ => new List<(DateTime Start, DateTime End)>());
        var activeEmployees = employees.Where(e => e.IsActive).ToList();
        var typeWeights = MeetingTypes.Select(t => t.Weight).ToArray();

        // working days from ~2 weeks back to ~2 weeks ahead (a realistic office load per room);
        // the current and next week are the busiest
        var days = Enumerable.Range(-12, 26).Select(o => todayLocal.AddDays(o))
            .Where(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday)).ToList();
        var dayWeights = days.Select(d => (d - todayLocal).TotalDays is >= -2 and <= 9 ? 3 : 2).ToArray();

        var created = 0;
        var attempts = 0;
        while (created < count && attempts++ < count * 40)
        {
            var type = MeetingTypes[WeightedIndex(typeWeights)];
            var day = days[WeightedIndex(dayWeights)];
            var duration = faker.PickRandom(type.Durations);
            var latestStart = 18 * 60 - duration;
            var startMinutes = 8 * 60 + _rnd.Next(0, Math.Max(1, (latestStart - 8 * 60) / 15 + 1)) * 15;
            var startLocal = day.AddMinutes(startMinutes);
            var start = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), tz);
            var end = start.AddMinutes(duration);
            var people = _rnd.Next(type.MinPeople, type.MaxPeople + 1);

            var buildingId = buildingIds[WeightedIndex(buildingWeights)];
            var isFuture = start > nowUtc;
            var candidates = roomsByBuilding[buildingId]
                .Where(r => (r.IsActive || !isFuture) && IsFree(occupancy[r.Id], start, end))
                .ToList();

            List<Room> picked;
            if (type.MultiRoom && _rnd.NextDouble() < 0.6)
            {
                // an overflow booking: the largest free room plus a second large one, video-linked
                picked = candidates.Where(r => r.Capacity >= 14).OrderByDescending(r => r.Capacity).Take(2).ToList();
                if (picked.Count < 2 || picked.Sum(r => r.Capacity) < people) continue;
            }
            else
            {
                // the smallest free room that fits (with a little randomness among the best fits)
                var fits = candidates.Where(r => r.Capacity >= people && r.Capacity <= Math.Max(people * 3, people + 4)).OrderBy(r => r.Capacity).Take(3).ToList();
                if (fits.Count == 0) continue;
                picked = [fits[_rnd.Next(fits.Count)]];
            }

            var organizer = activeEmployees[_rnd.Next(activeEmployees.Count)];
            var attendeeCount = Math.Min(people - 1, picked.Sum(r => r.Capacity) - 1);
            var colleagues = activeEmployees.Where(e => e.Id != organizer.Id).OrderBy(e => e.Department == organizer.Department ? 0 : 1).ThenBy(_ => _rnd.Next()).ToList();
            var sameDept = (int)Math.Round(attendeeCount * 0.7);
            var attendees = colleagues.Take(sameDept)
                .Concat(colleagues.Skip(sameDept).OrderBy(_ => _rnd.Next()).Take(attendeeCount - sameDept))
                .Take(attendeeCount).ToList();

            // {0}: the other person of a 1:1, the organizer's team for stand-ups, a candidate for interviews, else a topic
            var fill = type == MeetingTypes[0] ? $"with {attendees.FirstOrDefault()?.FirstName ?? "a colleague"}"
                : type == MeetingTypes[1] ? organizer.Department ?? "team"
                : type == MeetingTypes[3] ? faker.Name.FullName()
                : faker.PickRandom(Topics);
            var title = string.Format(faker.PickRandom(type.Titles), fill);

            var createdOn = start.AddDays(-_rnd.Next(1, 22)).AddHours(-_rnd.Next(0, 9));
            if (createdOn > nowUtc) createdOn = nowUtc.AddHours(-_rnd.Next(1, 48));

            // cancellations (~7%): cancelled some time between booking and start
            var cancelled = _rnd.NextDouble() < 0.07;
            var reservation = new Reservation
            {
                Title = title.Length > 128 ? title[..128] : title,
                Description = _rnd.NextDouble() < 0.6 ? string.Format(faker.PickRandom(Agendas), faker.PickRandom(Topics).ToLowerInvariant()) : null,
                OrganizerId = organizer.Id,
                Start = start,
                End = end,
                Status = cancelled ? ReservationStatus.Cancelled : ReservationStatus.Pending,
                CancelledOn = cancelled ? Min(createdOn.AddHours(_rnd.Next(2, 72)), start.AddHours(-1), nowUtc) : null,
                CancelReason = cancelled ? faker.PickRandom("Organizer unavailable", "Moved online", "Rescheduled", "No longer needed", "Double booking") : null,
                Created = createdOn,
                LastModified = cancelled ? null : (_rnd.NextDouble() < 0.3 ? Min(createdOn.AddHours(_rnd.Next(1, 48)), nowUtc) : null),
                Rooms = picked.Select(r => new ReservationRoom
                {
                    RoomId = r.Id,
                    ApprovalStatus = DecideApproval(r, isFuture),
                }).ToList(),
                Attendees = attendees.Select(a => new ReservationAttendee
                {
                    EmployeeId = a.Id,
                    IsOptional = _rnd.NextDouble() < 0.12,
                    Response = isFuture
                        ? (AttendeeResponse)WeightedIndex([35, 45, 10, 10])
                        : (AttendeeResponse)WeightedIndex([5, 80, 7, 8]),
                }).ToList(),
            };
            foreach (var row in reservation.Rooms!)
            {
                var room = picked.First(r => r.Id == row.RoomId);
                if (!room.RequiresApproval) { row.DecidedOn = createdOn; row.DecisionNote = "Automatically approved"; }
                else if (row.ApprovalStatus != RoomApprovalStatus.Pending)
                {
                    row.DecidedOn = Min(createdOn.AddHours(_rnd.Next(1, 30)), nowUtc);
                    row.DecisionNote = row.ApprovalStatus == RoomApprovalStatus.Rejected
                        ? faker.PickRandom("Reserved for management", "Maintenance planned", "Please book a smaller room", "Conflicts with a facility event")
                        : faker.PickRandom<string?>(null, "OK", "Approved - catering can be ordered via Operations");
                }
            }

            try
            {
                await reservationService.Add(reservation, token);
            }
            catch (Regira.Entities.Models.EntityInputException ex)
            {
                logger.LogError("Seeding reservation '{Title}' failed: {Errors}", reservation.Title,
                    string.Join("; ", ex.InputErrors.Select(kv => $"{kv.Key}: {kv.Value}")));
                throw;
            }
            if (!cancelled)
                foreach (var row in reservation.Rooms.Where(x => x.ApprovalStatus != RoomApprovalStatus.Rejected))
                    occupancy[row.RoomId].Add((start, end));
            created++;
            if (created % 100 == 0) await reservationService.SaveChanges(token);
        }
        await reservationService.SaveChanges(token);
        return created;
    }

    RoomApprovalStatus DecideApproval(Room room, bool isFuture)
    {
        if (!room.RequiresApproval) return RoomApprovalStatus.Approved;
        var roll = _rnd.NextDouble();
        if (isFuture) return roll < 0.5 ? RoomApprovalStatus.Pending : roll < 0.9 ? RoomApprovalStatus.Approved : RoomApprovalStatus.Rejected;
        return roll < 0.85 ? RoomApprovalStatus.Approved : RoomApprovalStatus.Rejected;
    }

    static bool IsFree(List<(DateTime Start, DateTime End)> slots, DateTime start, DateTime end)
        => !slots.Any(s => s.Start < end && s.End > start);

    static DateTime Min(params DateTime[] values) => values.Min();

    int WeightedIndex(int[] weights)
    {
        var roll = _rnd.Next(weights.Sum());
        for (var i = 0; i < weights.Length; i++)
        {
            if (roll < weights[i]) return i;
            roll -= weights[i];
        }
        return weights.Length - 1;
    }

    static string Slug(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => char.IsLetter(c) && c < 128).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }
}
