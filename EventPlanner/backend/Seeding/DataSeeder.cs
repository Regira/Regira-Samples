using Bogus;
using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.EventCategories;
using EventPlanner.Api.Entities.Employees;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Sessions;
using EventPlanner.Api.Entities.Speakers;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace EventPlanner.Api.Seeding;

// Seeds sample data through IEntityService (never the DbContext directly) so the normal Regira write
// pipeline — preppers, primers, Related() sync — runs exactly as it would for a real client request.
public static class DataSeeder
{
    private const int Seed = 20260819;

    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var dbContext = sp.GetRequiredService<EventPlannerDbContext>();

        // Idempotent: skip when the primary entity already has rows (re-running the app must not duplicate data).
        if (await dbContext.Registrations.AnyAsync())
        {
            return;
        }

        Randomizer.Seed = new Random(Seed);

        var categories = await SeedEventCategories(sp);
        var locations = await SeedLocations(sp);
        var speakers = await SeedSpeakers(sp);
        var employees = await SeedEmployees(sp);
        var events = await SeedEvents(sp, locations, categories);
        var sessions = await SeedSessions(sp, events, speakers);
        await SeedRegistrations(sp, employees, events, sessions);
    }

    // Bogus dates come back with DateTime.Kind = Unspecified; the entity pipeline's UTC convention
    // expects Kind = Utc, so stamp it explicitly rather than ToUniversalTime() (which would shift the
    // clock time as though the Unspecified value were Local).
    private static DateTime AsUtc(DateTime dt) => DateTime.SpecifyKind(dt, DateTimeKind.Utc);

    private static DateTime PastCreated(Faker f, int maxDaysAgo = 180) => AsUtc(f.Date.Recent(maxDaysAgo));

    private static async Task<List<EventCategory>> SeedEventCategories(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<EventCategory, int>>();
        var data = new (string Title, string Color, string Icon)[]
        {
            ("Technology", "#4361ee", "cpu"),
            ("Business", "#f72585", "briefcase"),
            ("Health & Wellness", "#06d6a0", "heart-pulse"),
            ("Marketing", "#ff9f1c", "megaphone"),
            ("Design", "#7209b7", "palette"),
            ("Leadership", "#3a0ca3", "people"),
            ("Finance", "#2a9d8f", "cash-coin"),
            ("Sustainability", "#588157", "tree"),
        };

        var items = data.Select(d => new EventCategory { Title = d.Title, ColorHex = d.Color, Icon = d.Icon }).ToList();
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Location>> SeedLocations(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Location, int>>();
        var faker = new Faker<Location>()
            .UseSeed(Seed)
            .RuleFor(x => x.Title, f => f.Company.CompanyName() + " " + f.PickRandom("Conference Center", "Convention Hall", "Auditorium", "Campus", "Hub", "Pavilion"))
            .RuleFor(x => x.Description, f => f.Lorem.Sentence(12))
            .RuleFor(x => x.Address, f => f.Address.StreetAddress())
            .RuleFor(x => x.City, f => f.Address.City())
            .RuleFor(x => x.PostalCode, f => f.Address.ZipCode())
            .RuleFor(x => x.Country, f => f.Address.Country())
            .RuleFor(x => x.Capacity, f => f.Random.Int(50, 2000))
            .RuleFor(x => x.ImageUrl, f => $"https://picsum.photos/seed/venue{f.IndexGlobal}/800/450")
            .RuleFor(x => x.Created, f => PastCreated(f));

        var items = faker.Generate(18);
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Speaker>> SeedSpeakers(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Speaker, int>>();
        var faker = new Faker<Speaker>()
            .UseSeed(Seed)
            .RuleFor(x => x.Title, f => f.Name.FullName())
            .RuleFor(x => x.Description, f => f.Lorem.Paragraph(2))
            .RuleFor(x => x.JobTitle, f => f.Name.JobTitle())
            .RuleFor(x => x.Company, f => f.Company.CompanyName())
            .RuleFor(x => x.Email, (f, x) => f.Internet.Email(x.Title))
            .RuleFor(x => x.PhotoUrl, f => $"https://i.pravatar.cc/300?img={f.Random.Int(1, 70)}")
            .RuleFor(x => x.Created, f => PastCreated(f));

        var items = faker.Generate(65);
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Employee>> SeedEmployees(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Employee, int>>();
        var departments = new[] { "Engineering", "Sales", "Marketing", "HR", "Finance", "Operations", "Customer Success", "Product", "Legal", "IT" };
        var faker = new Faker<Employee>()
            .UseSeed(Seed)
            .RuleFor(x => x.Title, f => f.Name.FullName())
            .RuleFor(x => x.Email, (f, x) => f.Internet.Email(x.Title, provider: "regira-demo.com"))
            .RuleFor(x => x.Department, f => f.PickRandom(departments))
            .RuleFor(x => x.JobTitle, f => f.Name.JobTitle())
            .RuleFor(x => x.AvatarUrl, f => $"https://i.pravatar.cc/150?img={f.Random.Int(1, 70)}")
            .RuleFor(x => x.Created, f => PastCreated(f));

        var items = faker.Generate(320);
        // De-dupe emails within the wave — an in-memory check, per §Seeding: uniqueness within one wave
        // must be checked in memory (a DB query can't see queued-but-unsaved rows).
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var deduped = new List<Employee>();
        foreach (var item in items)
        {
            if (seen.Add(item.Email)) deduped.Add(item);
        }

        foreach (var item in deduped) await service.Add(item);
        await service.SaveChanges();
        return deduped;
    }

    private static async Task<List<Event>> SeedEvents(IServiceProvider sp, List<Location> locations, List<EventCategory> categories)
    {
        var service = sp.GetRequiredService<IEntityService<Event, int>>();
        var titleTopics = new[]
        {
            "Summit", "Conference", "Forum", "Expo", "Symposium", "Workshop Series", "Meetup", "Bootcamp", "Roadshow", "Retreat"
        };
        var titleSubjects = new[]
        {
            "Cloud Innovation", "Future of Work", "Digital Marketing", "Product Design", "Leadership Excellence",
            "AI & Automation", "Sustainable Business", "Financial Growth", "Customer Experience", "Data Science",
            "Agile Practices", "Employee Wellbeing", "Startup Founders", "Cybersecurity", "Green Tech",
            "Remote Teams", "Brand Strategy", "DevOps Culture", "Supply Chain", "Diversity & Inclusion"
        };

        var faker = new Faker<Event>()
            .UseSeed(Seed)
            .RuleFor(x => x.Title, f => $"{f.PickRandom(titleSubjects)} {f.PickRandom(titleTopics)} {f.Date.Future(1).Year}")
            .RuleFor(x => x.Description, f => f.Lorem.Paragraphs(2, "\n\n"))
            .RuleFor(x => x.BannerImageUrl, f => $"https://picsum.photos/seed/event{f.IndexGlobal}/1200/500")
            .RuleFor(x => x.LocationId, f => f.PickRandom(locations).Id)
            .RuleFor(x => x.EventCategoryId, f => f.PickRandom(categories).Id)
            // Spread across a wide window so upcoming/ongoing/past buckets are all populated (avoid a
            // degenerate 0%/100% distribution on any "status" the UI derives from these dates).
            .RuleFor(x => x.StartDate, f => AsUtc(f.Date.Between(DateTime.UtcNow.AddDays(-120), DateTime.UtcNow.AddDays(150)).Date.AddHours(9)))
            .RuleFor(x => x.IsFeatured, f => f.Random.Bool(0.2f))
            .RuleFor(x => x.Created, f => PastCreated(f));

        var items = faker.Generate(70);
        foreach (var item in items)
        {
            // Multi-day span: 0 (single day) to 3 extra days.
            var extraDays = new Faker(locale: "en").Random.Int(0, 3);
            item.EndDate = item.StartDate.AddDays(extraDays).AddHours(8); // ends at ~17:00 on the last day
        }

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Session>> SeedSessions(IServiceProvider sp, List<Event> events, List<Speaker> speakers)
    {
        var service = sp.GetRequiredService<IEntityService<Session, int>>();
        var sessionTitles = new[]
        {
            "Opening Keynote", "Panel Discussion", "Deep Dive Workshop", "Lightning Talks", "Fireside Chat",
            "Hands-on Lab", "Roundtable", "Case Study Review", "Networking Break", "Closing Remarks",
            "Product Demo", "Strategy Masterclass", "Q&A Session", "Breakout Session", "Innovation Showcase"
        };
        var rooms = new[] { "Main Hall", "Room A", "Room B", "Room C", "Terrace", "Studio 1", "Studio 2", "Auditorium" };
        var rng = new Random(Seed);
        var faker = new Faker(locale: "en");

        var sessions = new List<Session>();
        foreach (var evt in events)
        {
            var dayCount = Math.Max(1, (evt.EndDate.Date - evt.StartDate.Date).Days + 1);
            var sessionCount = rng.Next(2, 6);
            for (var i = 0; i < sessionCount; i++)
            {
                var day = rng.Next(0, dayCount);
                var startHour = rng.Next(9, 16);
                var start = evt.StartDate.Date.AddDays(day).AddHours(startHour);
                var end = start.AddMinutes(faker.PickRandom(30, 45, 60, 90));

                var speakerCount = rng.Next(1, 4);
                var sessionSpeakers = speakers.OrderBy(_ => rng.Next()).Take(speakerCount)
                    .Select(sp2 => new SessionSpeaker { SpeakerId = sp2.Id })
                    .ToList();

                sessions.Add(new Session
                {
                    EventId = evt.Id,
                    Title = $"{faker.PickRandom(sessionTitles)}",
                    Description = faker.Lorem.Sentence(15),
                    Room = faker.PickRandom(rooms),
                    StartTime = start,
                    EndTime = end,
                    Capacity = faker.Random.Int(20, 300),
                    SessionSpeakers = sessionSpeakers,
                    Created = evt.Created,
                });
            }
        }

        foreach (var item in sessions) await service.Add(item);
        await service.SaveChanges();
        return sessions;
    }

    private static async Task SeedRegistrations(IServiceProvider sp, List<Employee> employees, List<Event> events, List<Session> sessions)
    {
        var service = sp.GetRequiredService<IEntityService<Registration, int>>();
        var rng = new Random(Seed);
        var faker = new Faker(locale: "en");

        var sessionsByEvent = sessions.GroupBy(x => x.EventId).ToDictionary(g => g.Key, g => g.ToList());

        // Realistic status mix — avoid every row landing in one bucket (Confirmed-heavy, but every
        // status represented so a dashboard filter on any of them returns non-zero rows).
        var statusWeights = new (RegistrationStatus Status, int Weight)[]
        {
            (RegistrationStatus.Confirmed, 50),
            (RegistrationStatus.Pending, 20),
            (RegistrationStatus.Attended, 20),
            (RegistrationStatus.Cancelled, 10),
        };
        RegistrationStatus PickStatus()
        {
            var total = statusWeights.Sum(x => x.Weight);
            var roll = rng.Next(total);
            var cumulative = 0;
            foreach (var (status, weight) in statusWeights)
            {
                cumulative += weight;
                if (roll < cumulative) return status;
            }
            return RegistrationStatus.Pending;
        }

        var seenPairs = new HashSet<(int EmployeeId, int EventId)>();
        var registrations = new List<Registration>();
        const int target = 520;
        var attempts = 0;
        while (registrations.Count < target && attempts < target * 6)
        {
            attempts++;
            var employee = employees[rng.Next(employees.Count)];
            var evt = events[rng.Next(events.Count)];
            if (!seenPairs.Add((employee.Id, evt.Id))) continue;

            var eventSessions = sessionsByEvent.GetValueOrDefault(evt.Id, []);
            var pickCount = eventSessions.Count == 0 ? 0 : rng.Next(0, Math.Min(4, eventSessions.Count) + 1);
            var selected = eventSessions.OrderBy(_ => rng.Next()).Take(pickCount)
                .Select(s => new RegistrationSession { SessionId = s.Id })
                .ToList();

            registrations.Add(new Registration
            {
                EmployeeId = employee.Id,
                EventId = evt.Id,
                Status = PickStatus(),
                Notes = faker.Random.Bool(0.15f) ? faker.Lorem.Sentence(8) : null,
                SelectedSessions = selected,
                Created = AsUtc(faker.Date.Between(evt.Created, DateTime.UtcNow)),
            });
        }

        foreach (var item in registrations) await service.Add(item);
        await service.SaveChanges();
    }
}
