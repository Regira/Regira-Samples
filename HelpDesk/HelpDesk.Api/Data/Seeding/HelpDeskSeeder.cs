using System.Text;
using Bogus;
using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Persons;
using Person = HelpDesk.Api.Entities.Persons.Person;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using HelpDesk.Api.Entities.Tickets;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Services.Abstractions;

namespace HelpDesk.Api.Data.Seeding;

/// <summary>
/// Seeds every entity through its IEntityService (preppers, primers and normalizers run as for the API),
/// accounts through UserManager. Runs once, on an empty database. Deterministic (fixed Bogus seed).
/// </summary>
public static class HelpDeskSeeder
{
    private const int TicketCount = 520;
    private const int CustomerCount = 160;
    private const int CustomerAccounts = 12;

    public static async Task SeedHelpDesk(this IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<HelpDeskDbContext>();
        if (await db.Tickets.AnyAsync()) return;

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("HelpDeskSeeder");
        var password = configuration["Seed:DemoPassword"]
            ?? throw new InvalidOperationException("Seed:DemoPassword is not configured (appsettings.Development.json).");

        // a seeder has no request: it is a trusted writer by explicit opt-in
        services.GetRequiredService<CurrentUser>().IsSystem = true;

        Randomizer.Seed = new Random(20260927);
        var f = new Faker("en");
        var now = DateTime.UtcNow;

        // --- reference data -------------------------------------------------------------------------------
        var teamService = services.GetRequiredService<IEntityService<SupportTeam, int>>();
        foreach (var t in SeedCatalog.Teams)
            await teamService.Add(new SupportTeam { Title = t.Title, Color = t.Color, Email = t.Email, Description = t.Description, Created = now.AddDays(-400) });
        await teamService.SaveChanges();
        var teamIds = await db.SupportTeams.AsNoTracking().ToDictionaryAsync(x => x.Title!, x => x.Id);

        var statusService = services.GetRequiredService<IEntityService<Status, int>>();
        (string Title, string Color, bool IsDefault, bool IsClosed, string Description)[] statuses =
        [
            ("New", "#0dcaf0", true, false, "Just reported, waiting for triage."),
            ("Open", "#0d6efd", false, false, "Triaged and queued for an agent."),
            ("In Progress", "#6f42c1", false, false, "An agent is working on it."),
            ("Waiting on Customer", "#ffc107", false, false, "We need a reply or an action from the customer."),
            ("Resolved", "#198754", false, true, "A fix was delivered; the customer can still reply."),
            ("Closed", "#6c757d", false, true, "Done and archived in the history.")
        ];
        for (var i = 0; i < statuses.Length; i++)
        {
            var s = statuses[i];
            await statusService.Add(new Status { Title = s.Title, Color = s.Color, IsDefault = s.IsDefault, IsClosed = s.IsClosed, Description = s.Description, SortOrder = (i + 1) * 10, Created = now.AddDays(-400) });
        }
        await statusService.SaveChanges();
        var statusIds = await db.Statuses.AsNoTracking().ToDictionaryAsync(x => x.Title!, x => x.Id);

        var priorityService = services.GetRequiredService<IEntityService<Priority, int>>();
        (string Title, int Level, string Color, int Hours, bool IsDefault, string Description)[] priorities =
        [
            ("Low", 1, "#6c757d", 120, false, "Questions and requests without business impact."),
            ("Normal", 2, "#0d6efd", 72, true, "One user impacted, a workaround exists."),
            ("High", 3, "#fd7e14", 24, false, "A team is impacted or no workaround exists."),
            ("Critical", 4, "#dc3545", 8, false, "Business stopped or security at risk.")
        ];
        foreach (var p in priorities)
            await priorityService.Add(new Priority { Title = p.Title, Level = p.Level, Color = p.Color, TargetHours = p.Hours, IsDefault = p.IsDefault, Description = p.Description, Created = now.AddDays(-400) });
        await priorityService.SaveChanges();
        var priorityList = await db.Priorities.AsNoTracking().OrderBy(x => x.Level).ToListAsync();

        var categoryService = services.GetRequiredService<IEntityService<Category, int>>();
        for (var i = 0; i < SeedCatalog.Categories.Length; i++)
        {
            var c = SeedCatalog.Categories[i];
            await categoryService.Add(new Category
            {
                Title = c.Title, Color = c.Color, Icon = c.Icon, Description = c.Description, SortOrder = (i + 1) * 10,
                SupportTeamId = teamIds[c.Team], Created = now.AddDays(-400)
            });
        }
        await categoryService.SaveChanges();
        var categoryIds = await db.Categories.AsNoTracking().ToDictionaryAsync(x => x.Title!, x => x.Id);

        // --- people ---------------------------------------------------------------------------------------
        var personService = services.GetRequiredService<IEntityService<Person, int>>();
        var employeeSeeds = new List<(Person Person, bool IsAdmin)>();
        var admin = new Person
        {
            Role = PersonRole.Employee, GivenName = "Alex", FamilyName = "Morgan", Email = "alex.morgan@helpdesk.test",
            Phone = "+32 2 555 01 00", JobTitle = "Support Center Manager", SupportTeamId = teamIds["Service Desk"], Created = now.AddDays(-400)
        };
        employeeSeeds.Add((admin, true));
        foreach (var team in SeedCatalog.Teams)
        {
            foreach (var jobTitle in team.JobTitles)
            {
                var given = f.Name.FirstName();
                var family = f.Name.LastName();
                employeeSeeds.Add((new Person
                {
                    Role = PersonRole.Employee, GivenName = given, FamilyName = family,
                    Email = $"{Slug(given)}.{Slug(family)}@helpdesk.test", Phone = f.Phone.PhoneNumber("+32 2 555 ## ##"),
                    JobTitle = jobTitle, SupportTeamId = teamIds[team.Title], Created = now.AddDays(-f.Random.Int(60, 400))
                }, false));
            }
        }
        foreach (var (person, _) in employeeSeeds) await personService.Add(person);

        var companies = Enumerable.Range(0, 32).Select(_ => f.Company.CompanyName()).Distinct().ToList();
        var customers = new List<Person>();
        var usedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (customers.Count < CustomerCount)
        {
            var company = f.PickRandom(companies);
            var given = f.Name.FirstName();
            var family = f.Name.LastName();
            var email = $"{Slug(given)}.{Slug(family)}@{Slug(company)}.test";
            if (!usedEmails.Add(email)) continue;   // in-memory dedupe within the wave
            customers.Add(new Person
            {
                Role = PersonRole.Customer, GivenName = given, FamilyName = family, Email = email, Company = company,
                Phone = f.Phone.PhoneNumber("+32 4## ## ## ##"), Created = now.AddDays(-f.Random.Int(30, 380)),
                IsActive = f.Random.Bool(0.95f)
            });
        }
        foreach (var customer in customers) await personService.Add(customer);
        await personService.SaveChanges();

        var people = await db.Persons.AsNoTracking().ToListAsync();
        var employeesByTeam = people.Where(p => p.Role == PersonRole.Employee)
            .GroupBy(p => p.SupportTeamId!.Value).ToDictionary(g => g.Key, g => g.ToList());
        var customerIds = people.Where(p => p.Role == PersonRole.Customer).OrderBy(p => p.Id).Select(p => p.Id).ToList();

        // --- accounts -------------------------------------------------------------------------------------
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var accountHolders = people.Where(p => p.Role == PersonRole.Employee)
            .Concat(people.Where(p => p.Role == PersonRole.Customer && p.IsActive).OrderBy(p => p.Id).Take(CustomerAccounts))
            .ToList();
        var links = new Dictionary<int, string>();
        foreach (var person in accountHolders)
        {
            var user = new AppUser { UserName = person.Email, Email = person.Email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Seeding user {person.Email} failed: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            if (person.Email == admin.Email)
                await userManager.AddToRolesAsync(user, [Roles.Admin, Roles.Agent]);
            else
                await userManager.AddToRoleAsync(user, person.Role == PersonRole.Employee ? Roles.Agent : Roles.Customer);
            links[person.Id] = user.Id;
        }
        // UserId is server-owned on the API (only restored on update); linking is a direct write here
        foreach (var person in await db.Persons.Where(p => links.Keys.Contains(p.Id)).ToListAsync())
            person.UserId = links[person.Id];
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // --- tickets --------------------------------------------------------------------------------------
        var weightedCategories = SeedCatalog.Categories.SelectMany(c => Enumerable.Repeat(c, c.Weight)).ToArray();
        var statusPlan = new (string Title, float Weight)[]
        {
            ("New", 0.07f), ("Open", 0.11f), ("In Progress", 0.13f), ("Waiting on Customer", 0.07f), ("Resolved", 0.17f), ("Closed", 0.45f)
        };
        var priorityPlan = new[] { 0.25f, 0.45f, 0.22f, 0.08f };   // Low, Normal, High, Critical

        var tickets = new List<Ticket>();
        for (var i = 0; i < TicketCount; i++)
        {
            var category = f.PickRandom(weightedCategories);
            var issue = f.PickRandom(category.Issues);
            var statusTitle = f.Random.WeightedRandom(statusPlan.Select(s => s.Title).ToArray(), statusPlan.Select(s => s.Weight).ToArray());
            var priority = f.Random.WeightedRandom(priorityList.ToArray(), priorityPlan);
            if (category.Title == "Security Incident" && priority.Level < 3) priority = priorityList[2 + f.Random.Int(0, 1)];
            // requests and admin chores are never urgent: correlate the priority with the issue
            if (IsRequest(issue.Subject) && priority.Level > 2) priority = priorityList[f.Random.Int(0, 1)];
            var isClosed = statusTitle is "Resolved" or "Closed";
            var teamId = teamIds[category.Team];
            var team = employeesByTeam[teamId];

            // open work is recent (so SLA badges stay meaningful); history spreads over half a year
            var created = statusTitle switch
            {
                // relative to each ticket's own SLA window, so roughly a quarter of the open work is overdue
                "New" => now.AddHours(-Math.Min(priority.TargetHours * f.Random.Double(0.02, 0.6), 36)),
                "Open" or "In Progress" or "Waiting on Customer" => now.AddHours(-priority.TargetHours * f.Random.Double(0.1, 1.35)),
                _ => now.AddHours(-f.Random.Double(24 * 2, 24 * 180))
            };
            var dueDate = created.AddHours(priority.TargetHours);
            DateTime? closedAt = isClosed
                ? Min(created.AddHours(priority.TargetHours * f.Random.Double(0.08, 1.35)), now.AddMinutes(-30))
                : null;
            var assignee = statusTitle == "New" && f.Random.Bool(0.8f) ? null : f.PickRandom(team);

            var customerId = f.Random.Bool(0.2f) ? customerIds[f.Random.Int(0, CustomerAccounts - 1)] : f.PickRandom(customerIds);
            var ticket = new Ticket
            {
                Title = issue.Subject,
                Description = issue.Body + "\n\n" + f.PickRandom(
                    "Thanks in advance.", "Kind regards.", "This is blocking my work today.", "Not urgent, but annoying.", "Please advise."),
                CustomerId = customerId,
                AssignedEmployeeId = assignee?.Id,
                SupportTeamId = teamId,
                PriorityId = priority.Id,
                StatusId = statusIds[statusTitle],
                Created = created,
                DueDate = dueDate,
                ClosedAt = closedAt,
                Categories = [new TicketCategory { CategoryId = categoryIds[category.Title] }]
            };
            if (f.Random.Bool(0.2f))
            {
                var second = f.PickRandom(SeedCatalog.Categories.Where(c => c.Team == category.Team && c.Title != category.Title).DefaultIfEmpty(category));
                if (second.Title != category.Title)
                    ticket.Categories.Add(new TicketCategory { CategoryId = categoryIds[second.Title] });
            }

            BuildConversation(f, ticket, statusTitle, assignee?.Id ?? f.PickRandom(team).Id, team, created, closedAt ?? now);
            tickets.Add(ticket);
        }

        // sequential codes in chronological order
        var ordered = tickets.OrderBy(t => t.Created).ToList();
        for (var i = 0; i < ordered.Count; i++) ordered[i].Code = TicketCodeGenerator.Format(i + 1);

        var ticketService = services.GetRequiredService<IEntityService<Ticket, int>>();
        foreach (var ticket in ordered) await ticketService.Add(ticket);
        await ticketService.SaveChanges();
        services.GetRequiredService<TicketCodeGenerator>().Reset();

        // --- attachments (through the link service: writes the files, fills Path/Length) -------------------
        var ticketIds = await db.Tickets.AsNoTracking().Select(t => t.Id).ToListAsync();
        var linkService = services.GetRequiredService<IEntityService<TicketAttachment, int>>();
        var png = Convert.FromBase64String(SeedCatalog.ScreenshotPngBase64);
        var attachmentCount = 0;
        foreach (var ticketId in ticketIds.Where(_ => f.Random.Bool(0.16f)))
        {
            foreach (var file in f.PickRandom(SeedCatalog.AttachmentFiles, f.Random.Int(1, 2)))
            {
                var bytes = file.ContentType == "image/png" ? png : Encoding.UTF8.GetBytes(FileContent(f, file.FileName));
                await linkService.Add(new TicketAttachment
                {
                    ObjectId = ticketId,
                    Attachment = new Attachment { FileName = file.FileName, ContentType = file.ContentType, Bytes = bytes }
                });
                attachmentCount++;
            }
        }
        await linkService.SaveChanges();

        logger.LogInformation("Seeded {Teams} teams, {Categories} categories, {People} people ({Accounts} accounts), {Tickets} tickets, {Attachments} attachments",
            teamIds.Count, categoryIds.Count, people.Count, links.Count, tickets.Count, attachmentCount);
    }

    private static void BuildConversation(Faker f, Ticket ticket, string status, int agentId, List<Person> team, DateTime start, DateTime end)
    {
        var comments = new List<TicketComment>();
        var span = Math.Max((end - start).TotalMinutes, 30);
        var cursor = start;
        DateTime Next(double share) => cursor = Min(cursor.AddMinutes(Math.Max(5, span * share * f.Random.Double(0.5, 1.5))), end);
        void Say(int authorId, string body, bool isInternal = false, double share = 0.2)
            => comments.Add(new TicketComment { AuthorId = authorId, Body = body, IsInternal = isInternal, Created = Next(share) });

        switch (status)
        {
            case "New":
                if (f.Random.Bool(0.3f)) Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerFollowUps), share: 0.4);
                break;
            case "Open":
                Say(agentId, f.PickRandom(SeedCatalog.AgentFirstReplies), share: 0.3);
                if (f.Random.Bool(0.5f)) Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerFollowUps), share: 0.3);
                break;
            case "In Progress":
                Say(agentId, f.PickRandom(SeedCatalog.AgentFirstReplies), share: 0.15);
                Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerFollowUps), share: 0.2);
                if (f.Random.Bool(0.35f)) Say(f.PickRandom(team).Id, f.PickRandom(SeedCatalog.InternalNotes), true, 0.15);
                Say(agentId, f.PickRandom(SeedCatalog.AgentProgress), share: 0.2);
                break;
            case "Waiting on Customer":
                Say(agentId, f.PickRandom(SeedCatalog.AgentFirstReplies), share: 0.15);
                Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerFollowUps), share: 0.2);
                Say(agentId, f.PickRandom(SeedCatalog.AgentWaiting), share: 0.2);
                break;
            default: // Resolved / Closed
                Say(agentId, f.PickRandom(SeedCatalog.AgentFirstReplies), share: 0.1);
                if (f.Random.Bool(0.7f)) Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerFollowUps), share: 0.15);
                if (f.Random.Bool(0.3f)) Say(f.PickRandom(team).Id, f.PickRandom(SeedCatalog.InternalNotes), true, 0.1);
                if (f.Random.Bool(0.5f)) Say(agentId, f.PickRandom(SeedCatalog.AgentProgress), share: 0.2);
                Say(agentId, f.PickRandom(SeedCatalog.AgentResolutions), share: 0.25);
                if (status == "Closed" && f.Random.Bool(0.6f)) Say(ticket.CustomerId, f.PickRandom(SeedCatalog.CustomerThanks), share: 0.1);
                break;
        }

        ticket.Comments = comments;
        ticket.FirstResponseAt = comments.FirstOrDefault(c => c.AuthorId != ticket.CustomerId && !c.IsInternal)?.Created;
        ticket.LastModified = comments.Count > 0 ? Max(comments.Max(c => c.Created), ticket.ClosedAt ?? DateTime.MinValue) : ticket.ClosedAt;
    }

    private static string FileContent(Faker f, string fileName) => fileName switch
    {
        "system-info.json" => $$"""{ "device": "{{f.Random.Replace("LT-####")}}", "os": "Windows 11 23H2", "ramGb": {{f.PickRandom(8, 16, 32)}}, "lastBoot": "{{DateTime.UtcNow.AddHours(-f.Random.Int(1, 90)):O}}" }""",
        "event-viewer-export.csv" => "Level,Date,Source,EventId\n" + string.Join('\n', Enumerable.Range(0, 6).Select(i =>
            $"{f.PickRandom("Error", "Warning", "Information")},{DateTime.UtcNow.AddMinutes(-i * 17):yyyy-MM-dd HH:mm},{f.PickRandom("Kernel-Power", "Outlook", "VPN Client", "Disk")},{f.Random.Int(1000, 9999)}")),
        _ => string.Join('\n', Enumerable.Range(0, 8).Select(i =>
            $"{DateTime.UtcNow.AddSeconds(-i * 41):yyyy-MM-dd HH:mm:ss} [{f.PickRandom("ERROR", "WARN", "INFO")}] {f.PickRandom("Connection timed out", "Retrying request", "Access denied for resource", "Handshake failed", "Service started")}"))
    };

    private static bool IsRequest(string subject)
        => new[] { "Request", "Please", "Guest", "Change", "Copy", "Downgrade", "Add five", "Need admin", "Update of", "New user", "Access request", "Mailbox almost", "Forgot password", "New phone" }
            .Any(prefix => subject.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));

    private static string Slug(string value)
        => new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;
}
