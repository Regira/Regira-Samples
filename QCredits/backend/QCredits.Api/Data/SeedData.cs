using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.Services.Abstractions;
using QCredits.Api.Entities.CreditPolicies;
using QCredits.Api.Entities.EmployeeCarryOvers;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Data;

/// <summary>
/// Seeds sample data through the registered IEntityService implementations (never through the raw
/// DbContext) so the same primers/preppers the API uses at runtime (TotalCredits recompute, the
/// QCreditRequestStatusPrimer trusted-writer path) also produce the seeded rows.
/// </summary>
public static class SeedData
{
    private static readonly string[] Departments =
    [
        "Engineering", "Sales", "Marketing", "Human Resources", "Finance",
        "Operations", "Customer Support", "Product", "Legal", "IT"
    ];

    private static readonly string[] Courses =
    [
        "Advanced C# and .NET", "Cloud Architecture on Azure", "Leadership Essentials",
        "Agile Project Management", "Data Analysis with Python", "Effective Communication",
        "Cybersecurity Fundamentals", "UX Design Principles", "Public Speaking",
        "Negotiation Skills", "Kubernetes in Practice", "Financial Modelling",
        "Sales Strategy Workshop", "Time Management Masterclass", "DevOps Fundamentals"
    ];

    private static readonly string[] Books =
    [
        "Clean Code", "The Pragmatic Programmer", "Atomic Habits", "Thinking, Fast and Slow",
        "The Lean Startup", "Deep Work", "Crucial Conversations", "Domain-Driven Design",
        "The Manager's Path", "Radical Candor"
    ];

    private static readonly string[] Subscriptions =
    [
        "Pluralsight", "O'Reilly Learning", "LinkedIn Learning", "Coursera Plus", "Udemy Business",
        "DataCamp", "A Cloud Guru", "Frontend Masters"
    ];

    private static readonly string[] SelfStudyTopics =
    [
        "TypeScript deep dive", "System design patterns", "Vue 3 composition API",
        "SQL performance tuning", "Accessibility guidelines", "Prompt engineering",
        "Docker & containerization", "GraphQL fundamentals"
    ];

    private static readonly string[] TrainingProviders =
    [
        "Regira Academy", "Skillsbuilder", "TechCampus", "LearnHub", "The Training Institute"
    ];

    public static async Task SeedAsync(IServiceProvider services, CancellationToken token = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        var employeeService = sp.GetRequiredService<IEntityService<Employee, int>>();
        var existing = await employeeService.Count(null, token);
        if (existing > 0)
        {
            return; // already seeded
        }

        var random = new Randomizer(20260819);
        Randomizer.Seed = new Random(20260819);

        var currentYear = DateTime.UtcNow.Year; // 2026
        int[] years = [currentYear - 2, currentYear - 1, currentYear];

        // ---- Employees ------------------------------------------------------------------
        var employeeFaker = new Faker<Employee>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Department, f => f.PickRandom(Departments))
            .RuleFor(x => x.JobTitle, f => f.Name.JobTitle())
            .RuleFor(x => x.HireDate, f => f.Date.Past(12, DateTime.UtcNow.AddYears(-1)))
            .RuleFor(x => x.IsActive, f => f.Random.Bool(0.95f))
            .RuleFor(x => x.Role, (f, x) => EmployeeRole.Employee);

        const int employeeCount = 160;
        const int adminCount = 14;
        var employees = employeeFaker.Generate(employeeCount);
        for (var i = 0; i < employees.Count; i++)
        {
            var e = employees[i];
            e.Email = $"{e.FirstName}.{e.LastName}{i}@qcredits-demo.test".ToLowerInvariant();
            if (i < adminCount)
            {
                e.Role = EmployeeRole.Admin;
            }
        }

        foreach (var e in employees)
        {
            await employeeService.Add(e, token);
        }
        await employeeService.SaveChanges(token);

        var admins = employees.Where(x => x.Role == EmployeeRole.Admin).ToList();

        // ---- Credit policies (one per year) ----------------------------------------------
        var policyService = sp.GetRequiredService<IEntityService<CreditPolicy, int>>();
        var policies = years.Select(y => new CreditPolicy
        {
            Year = y,
            AnnualCredits = 20m,
            ReservedCredits = 5m,
            MaxCarryOver = 10m,
            MinBalance = -10m
        }).ToList();
        foreach (var p in policies)
        {
            await policyService.Add(p, token);
        }
        await policyService.SaveChanges(token);

        // ---- Employee carry-overs (into the 2nd and 3rd seeded years, ~40% of employees) --
        var carryOverService = sp.GetRequiredService<IEntityService<EmployeeCarryOver, int>>();
        var carryOverWave = new List<EmployeeCarryOver>();
        foreach (var year in years.Skip(1))
        {
            foreach (var emp in employees)
            {
                if (random.Bool(0.4f))
                {
                    var amount = Math.Round(random.Decimal(0, 10) * 2, MidpointRounding.AwayFromZero) / 2; // 0.5 steps
                    carryOverWave.Add(new EmployeeCarryOver
                    {
                        EmployeeId = emp.Id,
                        Year = year,
                        CarriedOverCredits = amount,
                        Note = amount > 0 ? $"Carried over from {year - 1}" : null
                    });
                }
            }
        }
        foreach (var c in carryOverWave)
        {
            await carryOverService.Add(c, token);
        }
        await carryOverService.SaveChanges(token);

        // ---- Group trainings (funded separately, no impact on personal balances) ----------
        var groupTrainingService = sp.GetRequiredService<IEntityService<GroupTraining, int>>();
        var groupTrainingFaker = new Faker<GroupTraining>()
            .RuleFor(x => x.Title, f => $"{f.PickRandom(Courses)} - Group Session")
            .RuleFor(x => x.Description, f => f.Lorem.Sentences(2))
            .RuleFor(x => x.TrainingDate, f => f.Date.Between(DateTime.UtcNow.AddYears(-2), DateTime.UtcNow.AddMonths(6)))
            .RuleFor(x => x.Location, f => f.PickRandom("Head Office - Room A", "Head Office - Room B", "Online", "Conference Center", "Regional Office"))
            .RuleFor(x => x.Facilitator, f => f.PickRandom(TrainingProviders))
            .RuleFor(x => x.Cost, f => Math.Round(f.Random.Decimal(500, 8000), 2))
            .RuleFor(x => x.MaxParticipants, f => f.Random.Int(8, 40))
            .RuleFor(x => x.Department, f => f.Random.Bool(0.5f) ? f.PickRandom(Departments) : null);
        var groupTrainings = groupTrainingFaker.Generate(28);
        foreach (var g in groupTrainings)
        {
            await groupTrainingService.Add(g, token);
        }
        await groupTrainingService.SaveChanges(token);

        // ---- QCreditRequests (primary entity, ~500) + owned items -------------------------
        var requestWorkflow = sp.GetRequiredService<RequestWorkflowContext>();
        requestWorkflow.IsTrustedWriter = true; // seeder is a trusted writer, stamps historical decisions

        var requestService = sp.GetRequiredService<IEntityService<QCreditRequest, int>>();

        const int requestCount = 500;
        var requests = new List<QCreditRequest>(requestCount);
        for (var i = 0; i < requestCount; i++)
        {
            var employee = random.ArrayElement(employees.ToArray());
            // weight towards the two most recent years
            var year = random.WeightedRandom(years, [0.15f, 0.35f, 0.5f]);

            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = year == currentYear ? DateTime.UtcNow : new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            var submitted = RandomDate(random, yearStart, yearEnd < yearStart ? yearStart : yearEnd);

            var itemCount = random.Int(1, 3);
            var items = new List<QCreditRequestItem>(itemCount);
            for (var j = 0; j < itemCount; j++)
            {
                items.Add(BuildItem(random, submitted));
            }

            var statusRoll = random.Float();
            var status = statusRoll < 0.55f ? RequestStatus.Approved
                : statusRoll < 0.8f ? RequestStatus.Pending
                : RequestStatus.Rejected;

            var request = new QCreditRequest
            {
                EmployeeId = employee.Id,
                Year = year,
                SubmittedDate = submitted,
                Items = items,
                Status = status
            };

            if (status != RequestStatus.Pending)
            {
                var approver = random.ArrayElement(admins.ToArray());
                request.ApproverId = approver.Id;
                var decisionOffset = random.Int(1, 14);
                var decision = submitted.AddDays(decisionOffset);
                request.DecisionDate = decision > DateTime.UtcNow ? DateTime.UtcNow : decision;
                request.DecisionNotes = status == RequestStatus.Approved
                    ? "Approved - fits within the yearly budget."
                    : "Rejected - insufficient remaining QCredits for this period.";
            }

            requests.Add(request);
        }

        foreach (var r in requests)
        {
            await requestService.Add(r, token);
        }
        await requestService.SaveChanges(token);

        requestWorkflow.IsTrustedWriter = false;
    }

    private static DateTime RandomDate(Randomizer random, DateTime start, DateTime end)
    {
        if (end <= start)
        {
            return start;
        }
        var range = (end - start).TotalSeconds;
        return start.AddSeconds(random.Double(0, range));
    }

    private static QCreditRequestItem BuildItem(Randomizer random, DateTime submitted)
    {
        var type = random.Enum<CreditActivityType>();
        var credits = Math.Round(random.Decimal(0.5m, 6m) * 2, MidpointRounding.AwayFromZero) / 2; // 0.5 steps
        var activityDate = submitted.AddDays(-random.Int(0, 30));

        return type switch
        {
            CreditActivityType.Course => new QCreditRequestItem
            {
                Description = random.ArrayElement(Courses),
                Type = type,
                Credits = credits,
                ActivityDate = activityDate,
                Cost = Math.Round(credits * 250m + random.Decimal(-50, 150), 2),
                Provider = random.ArrayElement(TrainingProviders)
            },
            CreditActivityType.Book => new QCreditRequestItem
            {
                Description = $"Book: {random.ArrayElement(Books)}",
                Type = type,
                Credits = Math.Min(credits, 1m),
                ActivityDate = activityDate,
                Cost = Math.Round(random.Decimal(20, 60), 2),
                Provider = "Online bookstore"
            },
            CreditActivityType.Subscription => new QCreditRequestItem
            {
                Description = $"Subscription: {random.ArrayElement(Subscriptions)}",
                Type = type,
                Credits = credits,
                ActivityDate = activityDate,
                Cost = Math.Round(random.Decimal(150, 600), 2),
                Provider = random.ArrayElement(Subscriptions)
            },
            _ => new QCreditRequestItem
            {
                Description = $"Self-study: {random.ArrayElement(SelfStudyTopics)}",
                Type = type,
                Credits = credits,
                ActivityDate = activityDate,
                Cost = null,
                Provider = null
            }
        };
    }
}
