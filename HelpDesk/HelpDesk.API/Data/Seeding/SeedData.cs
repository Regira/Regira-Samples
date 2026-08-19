using Bogus;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Services.Abstractions;
using HelpDesk.API.Entities.Categories;
using HelpDesk.API.Entities.People;
using HelpDesk.API.Entities.Priorities;
using HelpDesk.API.Entities.Statuses;
using HelpDesk.API.Entities.SupportTeams;
using HelpDesk.API.Entities.Tickets;
using Person = HelpDesk.API.Entities.People.Person;

namespace HelpDesk.API.Data.Seeding;

public static class SeedData
{
    private const int TicketCount = 500;

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var dbContext = sp.GetRequiredService<AppDbContext>();

        if (await dbContext.Tickets.AnyAsync())
        {
            return; // already seeded
        }

        Randomizer.Seed = new Random(42024);

        var priorities = await SeedPriorities(sp);
        var statuses = await SeedStatuses(sp);
        var categories = await SeedCategories(sp);
        var teams = await SeedSupportTeams(sp);
        var people = await SeedPeople(sp, teams);
        var tickets = await SeedTickets(sp, dbContext, people, priorities, statuses, teams, categories);
        await SeedComments(dbContext, tickets, people);
        await SeedAttachments(sp, tickets);
    }

    private static async Task<List<Priority>> SeedPriorities(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Priority, int>>();
        var items = new List<Priority>
        {
            new() { Title = "Low", Level = 1, ColorHex = "#6c757d" },
            new() { Title = "Medium", Level = 2, ColorHex = "#0d6efd" },
            new() { Title = "High", Level = 3, ColorHex = "#fd7e14" },
            new() { Title = "Urgent", Level = 4, ColorHex = "#dc3545" }
        };
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Status>> SeedStatuses(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Status, int>>();
        var items = new List<Status>
        {
            new() { Title = "New", SortOrder = 1, IsClosed = false, ColorHex = "#6c757d" },
            new() { Title = "Open", SortOrder = 2, IsClosed = false, ColorHex = "#0dcaf0" },
            new() { Title = "In Progress", SortOrder = 3, IsClosed = false, ColorHex = "#0d6efd" },
            new() { Title = "On Hold", SortOrder = 4, IsClosed = false, ColorHex = "#ffc107" },
            new() { Title = "Resolved", SortOrder = 5, IsClosed = true, ColorHex = "#198754" },
            new() { Title = "Closed", SortOrder = 6, IsClosed = true, ColorHex = "#343a40" }
        };
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Category>> SeedCategories(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Category, int>>();
        var names = new[]
        {
            "Billing", "Technical Issue", "Account Access", "Feature Request", "Bug Report",
            "Hardware", "Software Installation", "Network & Connectivity", "Security", "General Inquiry"
        };
        var colors = new[] { "#0d6efd", "#dc3545", "#198754", "#fd7e14", "#6f42c1", "#20c997", "#0dcaf0", "#ffc107", "#d63384", "#6c757d" };
        var items = names.Select((n, i) => new Category { Title = n, Description = $"Tickets related to {n.ToLowerInvariant()}", ColorHex = colors[i] }).ToList();
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<SupportTeam>> SeedSupportTeams(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<SupportTeam, int>>();
        var items = new List<SupportTeam>
        {
            new() { Title = "Tier 1 Support", Description = "First line triage and general questions" },
            new() { Title = "Tier 2 Support", Description = "Escalated technical issues" },
            new() { Title = "Billing Team", Description = "Invoices, payments and subscriptions" },
            new() { Title = "Network Operations", Description = "Connectivity and infrastructure" },
            new() { Title = "Security Team", Description = "Access, security and compliance" }
        };
        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Person>> SeedPeople(IServiceProvider sp, List<SupportTeam> teams)
    {
        var service = sp.GetRequiredService<IEntityService<Person, int>>();
        var people = new List<Person>();

        var agentFaker = new Faker<Person>()
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FullName, provider: "helpdesk-support.example"))
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("+1-###-###-####"))
            .RuleFor(p => p.Role, _ => PersonRole.Agent)
            .RuleFor(p => p.JobTitle, f => f.PickRandom("Support Agent", "Senior Support Agent", "Support Engineer", "Team Lead"))
            .RuleFor(p => p.SupportTeamId, f => f.PickRandom(teams).Id)
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.92f))
            .RuleFor(p => p.Created, f => f.Date.Past(2));

        var customerFaker = new Faker<Person>()
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FullName))
            .RuleFor(p => p.Phone, f => f.Random.Bool(0.7f) ? f.Phone.PhoneNumber("+1-###-###-####") : null)
            .RuleFor(p => p.Role, _ => PersonRole.Customer)
            .RuleFor(p => p.Company, f => f.Random.Bool(0.6f) ? f.Company.CompanyName() : null)
            .RuleFor(p => p.IsActive, f => f.Random.Bool(0.95f))
            .RuleFor(p => p.Created, f => f.Date.Past(2));

        var adminFaker = new Faker<Person>()
            .RuleFor(p => p.FullName, f => f.Name.FullName())
            .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FullName, provider: "helpdesk-support.example"))
            .RuleFor(p => p.Phone, f => f.Phone.PhoneNumber("+1-###-###-####"))
            .RuleFor(p => p.Role, _ => PersonRole.Admin)
            .RuleFor(p => p.JobTitle, _ => "Help Desk Administrator")
            .RuleFor(p => p.IsActive, _ => true)
            .RuleFor(p => p.Created, f => f.Date.Past(2));

        people.AddRange(agentFaker.Generate(24));
        people.AddRange(customerFaker.Generate(320));
        people.AddRange(adminFaker.Generate(4));

        // de-dupe emails within this wave (in-memory - a DB uniqueness check can't see queued rows)
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var person in people)
        {
            var baseEmail = person.Email;
            var email = baseEmail;
            var suffix = 1;
            while (!seen.Add(email))
            {
                var at = baseEmail.IndexOf('@');
                email = $"{baseEmail[..at]}{suffix++}{baseEmail[at..]}";
            }
            person.Email = email;
        }

        foreach (var item in people) await service.Add(item);
        await service.SaveChanges();
        return people;
    }

    private static async Task<List<Ticket>> SeedTickets(
        IServiceProvider sp, AppDbContext dbContext, List<Person> people,
        List<Priority> priorities, List<Status> statuses, List<SupportTeam> teams, List<Category> categories)
    {
        var service = sp.GetRequiredService<IEntityService<Ticket, int>>();
        var customers = people.Where(p => p.Role == PersonRole.Customer).ToList();
        var agents = people.Where(p => p.Role == PersonRole.Agent).ToList();

        var subjects = new[]
        {
            "Unable to log into my account", "Invoice amount looks incorrect", "Application crashes on startup",
            "Feature request: dark mode support", "Cannot connect to VPN", "Password reset not working",
            "Slow performance when loading dashboard", "Need help configuring SSO", "Data export is missing columns",
            "Two-factor authentication not sending codes", "Billing address needs to be updated",
            "API returns 500 error intermittently", "Request to add new team member", "Mobile app keeps logging me out",
            "Report a suspicious login attempt", "How do I upgrade my subscription plan?",
            "File upload fails for large attachments", "Dashboard widgets not refreshing",
            "Need a refund for duplicate charge", "Printer integration not detected",
            "Email notifications are not arriving", "Request for additional storage quota",
            "Website displays broken layout on mobile", "Cannot download my invoice PDF",
            "Integration with third-party CRM failing", "Search results are outdated",
            "Unable to change my account email", "Sync between devices not working",
            "License key not being accepted", "Question about data retention policy"
        };

        var faker = new Faker();
        var tickets = new List<Ticket>();

        for (var i = 0; i < TicketCount; i++)
        {
            var status = PickStatus(faker, statuses);
            var priority = WeightedPriority(faker, priorities);
            var created = faker.Date.Between(DateTime.UtcNow.AddDays(-180), DateTime.UtcNow.AddDays(-1));
            var customer = faker.PickRandom(customers);

            // ~12% of tickets stay unassigned (mostly the newest ones), the rest go to an active agent
            var isUnassigned = status.Title == "New" && faker.Random.Bool(0.6f);
            var assignedEmployee = isUnassigned ? null : faker.PickRandom(agents.Where(a => a.IsActive).ToList());

            DateTime? closedAt = null;
            DateTime? lastModified = null;
            if (status.IsClosed)
            {
                closedAt = faker.Date.Between(created, DateTime.UtcNow);
                lastModified = closedAt;
            }
            else if (faker.Random.Bool(0.5f))
            {
                lastModified = faker.Date.Between(created, DateTime.UtcNow);
            }

            var ticket = new Ticket
            {
                Title = faker.PickRandom(subjects),
                Description = faker.Lorem.Paragraphs(faker.Random.Int(1, 3)),
                CustomerId = customer.Id,
                AssignedEmployeeId = assignedEmployee?.Id,
                PriorityId = priority.Id,
                StatusId = status.Id,
                SupportTeamId = assignedEmployee?.SupportTeamId ?? (faker.Random.Bool(0.4f) ? faker.PickRandom(teams).Id : null),
                Created = created,
                LastModified = lastModified,
                ClosedAt = closedAt,
                Categories = faker.PickRandom(categories, faker.Random.Int(1, 3)).Distinct()
                    .Select(c => new TicketCategory { CategoryId = c.Id }).ToList()
            };

            tickets.Add(ticket);
            await service.Add(ticket);
        }

        await service.SaveChanges();
        return tickets;
    }

    private static Status PickStatus(Faker faker, List<Status> statuses)
    {
        // Realistic queue shape: most tickets open/in-progress, a healthy share resolved/closed, few on hold
        var weights = new Dictionary<string, float>
        {
            ["New"] = 0.14f,
            ["Open"] = 0.16f,
            ["In Progress"] = 0.18f,
            ["On Hold"] = 0.07f,
            ["Resolved"] = 0.25f,
            ["Closed"] = 0.20f
        };
        return faker.Random.WeightedRandom(statuses.ToArray(), statuses.Select(s => weights.GetValueOrDefault(s.Title, 0.1f)).ToArray());
    }

    private static Priority WeightedPriority(Faker faker, List<Priority> priorities)
    {
        var weights = new Dictionary<string, float> { ["Low"] = 0.30f, ["Medium"] = 0.40f, ["High"] = 0.22f, ["Urgent"] = 0.08f };
        return faker.Random.WeightedRandom(priorities.ToArray(), priorities.Select(p => weights.GetValueOrDefault(p.Title, 0.25f)).ToArray());
    }

    private static async Task SeedComments(AppDbContext dbContext, List<Ticket> tickets, List<Person> people)
    {
        var faker = new Faker();
        var agents = people.Where(p => p.Role == PersonRole.Agent).ToList();
        var comments = new List<TicketComment>();

        foreach (var ticket in tickets)
        {
            var commentCount = faker.Random.Int(0, 5);
            if (commentCount == 0) continue;

            var when = ticket.Created;
            for (var i = 0; i < commentCount; i++)
            {
                when = when.AddHours(faker.Random.Double(1, 36));
                if (when > DateTime.UtcNow) when = DateTime.UtcNow;

                var isAgentTurn = i % 2 == 1 && agents.Count > 0;
                var author = isAgentTurn ? faker.PickRandom(agents) : people.First(p => p.Id == ticket.CustomerId);

                comments.Add(new TicketComment
                {
                    TicketId = ticket.Id,
                    AuthorId = author.Id,
                    Message = isAgentTurn
                        ? faker.PickRandom(
                            "Thanks for reaching out - looking into this now.",
                            "Could you provide a few more details or a screenshot?",
                            "This has been escalated to our technical team.",
                            "I've applied a fix, please confirm on your end.",
                            "Marking this as resolved - let us know if it happens again.")
                        : faker.PickRandom(
                            "Thanks for the quick response!",
                            "Here is some more information.",
                            "This is still happening on my end.",
                            "That worked, thank you!",
                            "Any update on this?"),
                    IsInternal = isAgentTurn && faker.Random.Bool(0.15f),
                    Created = when
                });
            }
        }

        dbContext.TicketComments.AddRange(comments);
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedAttachments(IServiceProvider sp, List<Ticket> tickets)
    {
        var links = sp.GetRequiredService<IEntityService<TicketAttachment, int>>();
        var faker = new Faker();
        var sample = tickets.OrderBy(_ => faker.Random.Int()).Take(90).ToList();

        foreach (var ticket in sample)
        {
            var text = $"Attachment for ticket #{ticket.Id} - {ticket.Title}\r\nGenerated for HelpDesk demo seed data.";
            var bytes = System.Text.Encoding.UTF8.GetBytes(text);
            await links.Add(new TicketAttachment
            {
                ObjectId = ticket.Id,
                Attachment = new Attachment
                {
                    FileName = faker.PickRandom("screenshot.txt", "log-export.txt", "details.txt", "error-report.txt"),
                    ContentType = "text/plain",
                    Bytes = bytes
                }
            });
        }

        await links.SaveChanges();
    }
}
