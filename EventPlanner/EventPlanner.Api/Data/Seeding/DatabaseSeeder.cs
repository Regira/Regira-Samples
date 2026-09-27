using Bogus;
using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Speakers;
using EventPlanner.Api.Entities.Users;
using EventPlanner.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace EventPlanner.Api.Data.Seeding;

public static class DatabaseInitializer
{
    /// <summary>Creates the SQLite schema (disposable DB, no migrations) and seeds it on first run.
    /// Pass --ResetDatabase=true to drop and re-create it.</summary>
    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var dbContext = sp.GetRequiredService<EventPlannerDbContext>();
        if (app.Configuration.GetValue<bool>("ResetDatabase"))
            await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        // the seeder runs without a request: give the scope an explicit trusted identity
        sp.GetRequiredService<CurrentUser>().RunAsSystem();
        await new DatabaseSeeder(sp, app.Configuration.GetValue("Seed:Events", 500)).Seed();
    }
}

/// <summary>Seeds every entity through its IEntityService (so preppers, normalizers and primers run), users through UserManager.</summary>
public class DatabaseSeeder(IServiceProvider sp, int eventCount)
{
    public const string AdminEmail = "admin@eventplanner.local";
    public const string AdminPassword = "Admin123!";
    public const string EmployeeEmail = "employee@eventplanner.local";
    public const string EmployeePassword = "Employee123!";
    public const string SharedEmployeePassword = "Welcome123!";

    private readonly EventPlannerDbContext _db = sp.GetRequiredService<EventPlannerDbContext>();
    private readonly ILogger _logger = sp.GetRequiredService<ILogger<DatabaseSeeder>>();
    private readonly Randomizer _rnd = new(20260927);
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task Seed()
    {
        await SeedUsers();
        if (await _db.Events.AnyAsync()) return;
        Randomizer.Seed = new Random(20260927);
        var started = DateTime.UtcNow;

        var categoryIds = await SeedCategories();
        var locations = await SeedLocations();
        var speakerIds = await SeedSpeakers();
        await SeedEvents(categoryIds, locations, speakerIds);
        await SeedRegistrations();

        _logger.LogInformation("Seeding done in {Seconds:0.0}s", (DateTime.UtcNow - started).TotalSeconds);
    }

    // ---------- users (ASP.NET Identity — not Regira entities) ----------
    private async Task SeedUsers()
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { Roles.Admin, Roles.Employee })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        if (await userManager.FindByEmailAsync(AdminEmail) != null) return;

        var admin = new AppUser { UserName = AdminEmail, Email = AdminEmail, EmailConfirmed = true, FirstName = "Alex", LastName = "Admin", Department = "HR", JobTitle = "Event Manager" };
        Check(await userManager.CreateAsync(admin, AdminPassword));
        Check(await userManager.AddToRolesAsync(admin, [Roles.Admin, Roles.Employee]));

        var demo = new AppUser { UserName = EmployeeEmail, Email = EmployeeEmail, EmailConfirmed = true, FirstName = "Emma", LastName = "Employee", Department = "Engineering", JobTitle = "Software Engineer" };
        Check(await userManager.CreateAsync(demo, EmployeePassword));
        Check(await userManager.AddToRoleAsync(demo, Roles.Employee));

        // hashing is deliberately slow: hash the shared password once and reuse it for the generated employees
        var sharedHash = userManager.PasswordHasher.HashPassword(demo, SharedEmployeePassword);
        var faker = new Faker("en");
        var usedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { AdminEmail, EmployeeEmail };
        for (var i = 0; i < 150; i++)
        {
            var (department, titles) = faker.PickRandom(SeedCatalog.Departments);
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var email = $"{Slug(first)}.{Slug(last)}@eventplanner.local";
            if (!usedEmails.Add(email)) email = $"{Slug(first)}.{Slug(last)}{i}@eventplanner.local";
            var user = new AppUser
            {
                UserName = email, Email = email, EmailConfirmed = true, FirstName = first, LastName = last,
                Department = department, JobTitle = faker.PickRandom(titles), PasswordHash = sharedHash
            };
            Check(await userManager.CreateAsync(user));
            Check(await userManager.AddToRoleAsync(user, Roles.Employee));
        }
        _logger.LogInformation("Seeded 152 users");
    }

    // ---------- reference data ----------
    private async Task<Dictionary<string, int>> SeedCategories()
    {
        var service = sp.GetRequiredService<IEntityService<EventCategory, int>>();
        foreach (var c in SeedCatalog.Categories)
            await service.Add(new EventCategory { Title = c.Title, Color = c.Color, Icon = c.Icon, Description = c.Description, Created = DateTime.UtcNow.AddYears(-2) });
        await service.SaveChanges();
        return await _db.EventCategories.AsNoTracking().ToDictionaryAsync(x => x.Title!, x => x.Id);
    }

    private async Task<List<(int Id, int Capacity)>> SeedLocations()
    {
        var service = sp.GetRequiredService<IEntityService<Location, int>>();
        foreach (var l in SeedCatalog.Locations)
            await service.Add(new Location
            {
                Title = l.Title, Address = l.Address, City = l.City, Country = l.Country, Capacity = l.Capacity,
                Description = l.Description, Created = DateTime.UtcNow.AddYears(-2)
            });
        await service.SaveChanges();
        return (await _db.Locations.AsNoTracking().Select(x => new { x.Id, x.Capacity }).ToListAsync())
            .Select(x => (x.Id, x.Capacity)).ToList();
    }

    private async Task<List<int>> SeedSpeakers()
    {
        var service = sp.GetRequiredService<IEntityService<Speaker, int>>();
        var faker = new Faker("en");
        for (var i = 0; i < 80; i++)
        {
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var company = faker.Company.CompanyName();
            var topics = faker.PickRandom(SeedCatalog.SpeakerTopics, faker.Random.Int(1, 3)).ToList();
            var jobTitle = faker.Name.JobTitle();
            await service.Add(new Speaker
            {
                FirstName = first, LastName = last, Company = company, JobTitle = jobTitle,
                Email = $"{Slug(first)}.{Slug(last)}@{Slug(company.Split(' ')[0])}.example",
                Topics = string.Join(", ", topics),
                Bio = $"{first} is {jobTitle} at {company} and speaks about {string.Join(" and ", topics)}. " + faker.Lorem.Sentences(2),
                Created = DateTime.UtcNow.AddDays(-faker.Random.Int(30, 700))
            });
        }
        await service.SaveChanges();
        return await _db.Speakers.AsNoTracking().Select(x => x.Id).ToListAsync();
    }

    // ---------- events (primary entity) + owned sessions + speaker join rows ----------
    private async Task SeedEvents(Dictionary<string, int> categoryIds, List<(int Id, int Capacity)> locations, List<int> speakerIds)
    {
        var service = sp.GetRequiredService<IEntityService<Event, int>>();
        var faker = new Faker("en");
        for (var i = 0; i < eventCount; i++)
        {
            var cat = faker.PickRandom(SeedCatalog.Categories);
            // spread over ~10 months back and ~8 months ahead
            var start = _today.AddDays(faker.Random.Int(-300, 240));
            var days = faker.Random.Int(cat.MinDays, cat.MaxDays);
            var end = start.AddDays(days - 1);
            var topic = faker.PickRandom(cat.Topics);
            var title = string.Format(faker.PickRandom(cat.TitleTemplates), topic, start.Year);

            // a venue that fits the category size (fall back to the largest)
            var size = faker.Random.Int(cat.MinSize, cat.MaxSize);
            var fitting = locations.Where(l => l.Capacity >= size && l.Capacity <= size * 4).ToList();
            var location = fitting.Count > 0 ? faker.PickRandom(fitting) : locations.OrderByDescending(l => l.Capacity).First();
            var max = Math.Min(size, location.Capacity);

            var isPast = end < _today;
            var status = faker.Random.Double() switch
            {
                < 0.06 when !isPast => EventStatus.Draft,
                < 0.10 => EventStatus.Cancelled,
                _ => EventStatus.Published
            };

            var created = start.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc).AddDays(-faker.Random.Int(20, 150));
            if (created > DateTime.UtcNow) created = DateTime.UtcNow.AddDays(-faker.Random.Int(0, 10));

            var ev = new Event
            {
                Title = title,
                Summary = $"{cat.Description.TrimEnd('.')} - {topic}.",
                Description = $"Join us for {title}. " + faker.Lorem.Paragraphs(2),
                CategoryId = categoryIds[cat.Title],
                LocationId = location.Id,
                StartDate = start,
                EndDate = end,
                Status = status,
                IsFeatured = status == EventStatus.Published && !isPast && faker.Random.Double() < 0.12,
                MaxParticipants = max,
                Created = created,
                Sessions = BuildSessions(faker, cat, topic, start, days, max, speakerIds)
            };
            await service.Add(ev);
            if ((i + 1) % 100 == 0) await service.SaveChanges();
        }
        await service.SaveChanges();
        _logger.LogInformation("Seeded {Count} events", eventCount);
    }

    private static List<Session> BuildSessions(Faker faker, SeedCatalog.CategorySeed cat, string topic, DateOnly start, int days, int max, List<int> speakerIds)
    {
        var sessions = new List<Session>();
        for (var d = 0; d < days; d++)
        {
            // 08:00 UTC = 09:00/10:00 Brussels time
            var time = start.AddDays(d).ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc);
            var perDay = faker.Random.Int(2, Math.Min(4, cat.SessionTemplates.Length));
            foreach (var template in cat.SessionTemplates.OrderBy(_ => faker.Random.Int()).Take(perDay))
            {
                var minutes = faker.PickRandom(45, 60, 90);
                var speakerCount = cat.MaxSpeakers == 0 ? 0 : faker.Random.Int(1, cat.MaxSpeakers);
                sessions.Add(new Session
                {
                    Title = string.Format(template, topic, start.Year + 1) + (days > 1 ? $" (Day {d + 1})" : ""),
                    Description = faker.Lorem.Sentences(2),
                    StartTime = time,
                    EndTime = time.AddMinutes(minutes),
                    Room = days == 1 && cat.Title == "Workshop" ? "Lab 1" : faker.PickRandom(SeedCatalog.Rooms),
                    Track = faker.PickRandom(SeedCatalog.Tracks),
                    Capacity = Math.Max(5, (int)(max * faker.Random.Double(0.5, 1.0))),
                    Speakers = faker.PickRandom(speakerIds, speakerCount).Select(id => new SessionSpeaker { SpeakerId = id }).ToList()
                });
                time = time.AddMinutes(minutes + faker.PickRandom(15, 30, 60));
            }
        }
        return sessions;
    }

    // ---------- registrations + owned session selections ----------
    private async Task SeedRegistrations()
    {
        var service = sp.GetRequiredService<IEntityService<Registration, int>>();
        var faker = new Faker("en");
        var userIds = await _db.Users.AsNoTracking().Where(u => u.Email != AdminEmail).Select(u => u.Id).ToListAsync();
        var demoId = await _db.Users.Where(u => u.Email == EmployeeEmail).Select(u => u.Id).FirstAsync();
        var events = await _db.Events.AsNoTracking()
            .Where(e => e.Status != EventStatus.Draft)
            .Select(e => new { e.Id, e.Status, e.StartDate, e.EndDate, e.MaxParticipants, e.Created })
            .ToListAsync();
        var sessionsByEvent = (await _db.Sessions.AsNoTracking().Select(s => new { s.Id, s.EventId, s.Capacity }).ToListAsync())
            .GroupBy(s => s.EventId).ToDictionary(g => g.Key, g => g.ToList());

        var total = 0;
        foreach (var ev in events)
        {
            var max = ev.MaxParticipants ?? 50;
            var isPast = ev.EndDate < _today;
            var daysAhead = ev.StartDate.DayNumber - _today.DayNumber;
            // popularity: past events filled up, far-away events are still filling
            var fill = isPast ? faker.Random.Double(0.5, 1.3) : daysAhead > 90 ? faker.Random.Double(0.05, 0.5) : faker.Random.Double(0.3, 1.3);
            var target = Math.Min((int)Math.Round(max * fill), faker.Random.Int(4, 45));
            var participants = faker.PickRandom(userIds, Math.Min(target, userIds.Count)).ToList();
            // the demo employee is registered for ~1 in 12 events so the "My registrations" page has content
            if (!participants.Contains(demoId) && faker.Random.Double() < 0.08) participants.Add(demoId);

            var sessions = sessionsByEvent.GetValueOrDefault(ev.Id) ?? [];
            var seatsTaken = sessions.ToDictionary(s => s.Id, _ => 0);
            var confirmed = 0;
            var eventStart = ev.StartDate.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc);
            foreach (var userId in participants)
            {
                RegistrationStatus status;
                if (ev.Status == EventStatus.Cancelled) status = RegistrationStatus.Cancelled;
                else if (faker.Random.Double() < 0.06) status = RegistrationStatus.Cancelled;
                else if (confirmed < max) { status = RegistrationStatus.Confirmed; confirmed++; }
                else status = RegistrationStatus.Waitlisted;

                var picks = new List<RegistrationSession>();
                if (sessions.Count > 1 && status != RegistrationStatus.Cancelled && faker.Random.Double() < 0.65)
                {
                    foreach (var s in faker.PickRandom(sessions, faker.Random.Int(1, sessions.Count)))
                        if (seatsTaken[s.Id] < s.Capacity)
                        {
                            seatsTaken[s.Id]++;
                            picks.Add(new RegistrationSession { SessionId = s.Id });
                        }
                }

                var latest = eventStart.AddDays(-1) < DateTime.UtcNow ? eventStart.AddDays(-1) : DateTime.UtcNow;
                var created = ev.Created < latest ? faker.Date.Between(ev.Created, latest) : ev.Created;
                await service.Add(new Registration
                {
                    EventId = ev.Id,
                    UserId = userId,
                    Status = status,
                    Notes = faker.Random.Double() < 0.15 ? faker.PickRandom("Vegetarian meal please.", "I will join remotely for the first part.", "Wheelchair access needed.", "Arriving a bit later.", "Happy to help with the setup.") : null,
                    Created = created,
                    Sessions = picks
                });
                total++;
            }
            if (total >= 800) { await service.SaveChanges(); total = 0; }
        }
        await service.SaveChanges();
        _logger.LogInformation("Seeded {Count} registrations", await _db.Registrations.CountAsync());
    }

    private static string Slug(string value) => new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static void Check(IdentityResult result)
    {
        if (!result.Succeeded) throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
