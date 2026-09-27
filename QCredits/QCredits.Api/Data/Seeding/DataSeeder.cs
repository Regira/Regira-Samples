using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Controllers;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.CreditYears;
using QCredits.Api.Entities.Departments;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Infrastructure.Security;
using Regira.Entities.Services.Abstractions;

namespace QCredits.Api.Data.Seeding;

/// <summary>
/// Seeds demo data through the IEntityService implementations (preppers, primers and normalizers run).
/// Acts as a trusted writer so it may stamp historical workflow states.
/// Primary entity: CreditRequest (~500 rows).
/// </summary>
public class DataSeeder(
    IServiceProvider services,
    AppDbContext db,
    WorkflowContext workflow,
    UserManager<AppUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IConfiguration configuration,
    ILogger<DataSeeder> logger)
{
    private static readonly DateTime Today = new(2026, 9, 27, 0, 0, 0, DateTimeKind.Utc);
    private static readonly int[] Years = [2024, 2025, 2026];
    private const int EmployeeCount = 90;

    private IEntityService<T, int> Service<T>() where T : class, Regira.Entities.Models.Abstractions.IEntity<int>
        => services.GetRequiredService<IEntityService<T, int>>();

    public async Task Seed(CancellationToken token = default)
    {
        if (await db.Departments.AnyAsync(token)) return;
        logger.LogInformation("Seeding demo data...");
        Randomizer.Seed = new Random(2026);
        var faker = new Faker("nl_BE");
        workflow.IsTrustedWriter = true;
        try
        {
            var departments = await SeedDepartments(token);
            var employees = await SeedEmployees(faker, departments, token);
            var policies = await SeedCreditYears(token);
            await SeedAllocationsAndRequests(faker, employees, policies, departments, token);
            await SeedGroupTrainings(faker, employees, departments, token);
            await SeedUsers(token);
        }
        finally
        {
            workflow.IsTrustedWriter = false;
        }
        logger.LogInformation("Seeding done.");
    }

    private async Task<Dictionary<int, string>> SeedDepartments(CancellationToken token)
    {
        var service = Service<Department>();
        foreach (var d in SeedCatalog.Departments)
            await service.Add(new Department { Title = d.Title, Code = d.Code, Description = d.Description });
        await service.SaveChanges(token);
        return await db.Departments.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Title!, token);
    }

    private record SeededEmployee(int Id, int DepartmentId, string Department, DateOnly HireDate, bool IsActive);

    private async Task<List<SeededEmployee>> SeedEmployees(Faker faker, Dictionary<int, string> departments, CancellationToken token)
    {
        var service = Service<Employee>();
        var departmentIds = departments.ToDictionary(x => x.Value, x => x.Key);
        var emails = new HashSet<string>();

        // fixed demo accounts (see README): an HR administrator and two employees
        var fixedPeople = new (string First, string Last, string Email, string Department, string Job)[]
        {
            ("Hanne", "Peeters", configuration["Seed:AdminEmail"] ?? "admin@qcredits.test", "Human Resources", "HR Business Partner"),
            ("Elise", "Maes", "elise.maes@qcredits.test", "Engineering", "Senior Software Engineer"),
            ("Tom", "Wouters", "tom.wouters@qcredits.test", "Sales", "Account Manager"),
        };
        foreach (var p in fixedPeople)
        {
            emails.Add(p.Email);
            await service.Add(new Employee
            {
                FirstName = p.First, LastName = p.Last, Email = p.Email, JobTitle = p.Job,
                DepartmentId = departmentIds[p.Department], HireDate = new DateOnly(2019, 3, 1), IsActive = true
            });
        }

        var weighted = SeedCatalog.Departments.Select(d => (d, (float)d.Weight)).ToArray();
        var weights = weighted.Select(x => x.Item2 / weighted.Sum(w => w.Item2)).ToArray();
        for (var i = fixedPeople.Length; i < EmployeeCount; i++)
        {
            var dept = faker.Random.WeightedRandom(SeedCatalog.Departments, weights);
            var first = faker.Name.FirstName();
            var last = faker.Name.LastName();
            var email = $"{Slug(first)}.{Slug(last)}@qcredits.test";
            for (var n = 2; !emails.Add(email); n++) email = $"{Slug(first)}.{Slug(last)}{n}@qcredits.test";
            // most staff joined before 2024; some joined during the demo years, a few have left
            var hireDate = faker.Random.Bool(0.8f)
                ? DateOnly.FromDateTime(faker.Date.Between(new DateTime(2012, 1, 1), new DateTime(2023, 12, 31)))
                : DateOnly.FromDateTime(faker.Date.Between(new DateTime(2024, 1, 1), new DateTime(2026, 6, 30)));
            await service.Add(new Employee
            {
                FirstName = first, LastName = last, Email = email,
                JobTitle = faker.PickRandom(dept.JobTitles),
                DepartmentId = departmentIds[dept.Title],
                HireDate = hireDate,
                IsActive = faker.Random.Bool(0.95f)
            });
        }
        await service.SaveChanges(token);

        return await db.Employees.AsNoTracking()
            .Select(x => new SeededEmployee(x.Id, x.DepartmentId, x.Department!.Title!, x.HireDate, x.IsActive))
            .ToListAsync(token);
    }

    private async Task<Dictionary<int, CreditYear>> SeedCreditYears(CancellationToken token)
    {
        var service = Service<CreditYear>();
        foreach (var year in Years)
            await service.Add(new CreditYear
            {
                Year = year, AnnualCredits = 20, ReservedCredits = 5, MaxCarryOver = 10, MinBalance = -10,
                IsClosed = year < 2026,
                Notes = year < 2026 ? "Closed; balances rolled over to the next year." : "Current year.",
                Created = new DateTime(year - 1, 12, 1, 9, 0, 0, DateTimeKind.Utc)
            });
        await service.SaveChanges(token);
        return await db.CreditYears.AsNoTracking().ToDictionaryAsync(x => x.Year, token);
    }

    private async Task SeedAllocationsAndRequests(Faker faker, List<SeededEmployee> employees,
        Dictionary<int, CreditYear> policies, Dictionary<int, string> departments, CancellationToken token)
    {
        var allocationService = Service<CreditAllocation>();
        var requestService = Service<CreditRequest>();
        var admins = new[] { "admin@qcredits.test", "an.hr@qcredits.test" };
        var carry = new Dictionary<int, double>();
        var total = 0;

        foreach (var year in Years)
        {
            var policy = policies[year];
            var yearStart = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var yearEnd = year == Today.Year ? Today : new DateTime(year, 12, 31, 0, 0, 0, DateTimeKind.Utc);
            var nextCarry = new Dictionary<int, double>();

            foreach (var employee in employees.Where(e => e.HireDate.Year <= year))
            {
                // a new joiner gets a pro-rata budget in the year they joined
                var factor = employee.HireDate.Year == year ? Math.Round((13 - employee.HireDate.Month) / 12d * 2, MidpointRounding.AwayFromZero) / 2 : 1;
                var annual = Math.Round(policy.AnnualCredits * factor * 2) / 2;
                var reserved = Math.Min(policy.ReservedCredits, annual);
                var carried = carry.GetValueOrDefault(employee.Id);
                var allocation = new CreditAllocation
                {
                    EmployeeId = employee.Id, Year = year,
                    AnnualCredits = annual, ReservedCredits = reserved, CarriedOver = carried, MinBalance = policy.MinBalance,
                    ReservedUsed = year < Today.Year ? reserved : Math.Min(reserved, faker.PickRandom(2d, 3d, 3d, 4d)),
                    Created = yearStart.AddDays(-20)
                };
                if (faker.Random.Bool(0.04f)) { allocation.MinBalance = -5; allocation.Notes = "Reduced overdraft limit agreed with HR."; }
                await allocationService.Add(allocation);

                var free = annual - reserved + carried;
                var used = 0d;
                var pending = 0d;
                var activeFrom = employee.HireDate.Year == year ? employee.HireDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc) : yearStart;
                var count = employee.IsActive || year < Today.Year
                    ? faker.Random.WeightedRandom(new[] { 0, 1, 2, 3, 4 }, new[] { .05f, .19f, .33f, .27f, .16f })
                    : 0;
                var topics = SeedCatalog.TopicsFor(employee.Department);

                for (var i = 0; i < count; i++)
                {
                    var topic = faker.PickRandom(topics);
                    var activities = faker.PickRandom(topic.Activities, faker.Random.Int(1, topic.Activities.Length)).ToList();
                    var created = faker.Date.Between(activeFrom, yearEnd.AddDays(year == Today.Year ? -1 : -20));
                    var request = new CreditRequest
                    {
                        EmployeeId = employee.Id,
                        Year = year,
                        Title = topic.Title,
                        Description = topic.Motivation,
                        Created = created,
                        Items = activities.Select((a, idx) => new CreditRequestItem
                        {
                            Type = a.Type, Description = a.Description, Provider = a.Provider, Url = a.Url,
                            Credits = a.Credits, Cost = a.Cost, SortOrder = idx,
                            ActivityDate = DateOnly.FromDateTime(faker.Date.Between(created, created.AddDays(90) > yearEnd.AddMonths(3) ? yearEnd : created.AddDays(90)))
                        }).ToList()
                    };
                    var credits = request.Items.Sum(x => x.Credits);
                    var remaining = free - used;
                    var status = PickStatus(faker, year, created);

                    // keep the data consistent with the rules: approval only within the minimum balance,
                    // a pending request only when it can still be requested
                    if (status == RequestStatus.Approved && remaining - credits < allocation.MinBalance)
                        status = RequestStatus.Rejected;
                    if (status == RequestStatus.Submitted && remaining - pending - credits < allocation.MinBalance)
                        status = RequestStatus.Draft;

                    request.Status = status;
                    if (status is RequestStatus.Submitted or RequestStatus.Approved or RequestStatus.Rejected or RequestStatus.Cancelled)
                        request.SubmittedAt = created.AddHours(faker.Random.Int(1, 72));
                    if (status is RequestStatus.Approved or RequestStatus.Rejected)
                    {
                        var decided = request.SubmittedAt!.Value.AddDays(faker.Random.Int(1, 10)).AddHours(faker.Random.Int(0, 8));
                        request.DecidedAt = decided > Today ? Today.AddHours(-faker.Random.Int(2, 30)) : decided;
                        request.DecidedBy = faker.PickRandom(admins);
                        request.DecisionComment = status == RequestStatus.Approved
                            ? faker.PickRandom<string?>(null, "Approved - enjoy the training!", "Approved. Please share your learnings with the team.", "OK for me.")
                            : remaining - credits < allocation.MinBalance
                                ? "Rejected: this request would take the balance below the minimum."
                                : faker.PickRandom("Not aligned with your development plan, let's discuss.", "Please look for a cheaper provider first.", "Covered by an upcoming group training.");
                    }
                    if (status == RequestStatus.Approved) used += credits;
                    if (status == RequestStatus.Submitted) pending += credits;
                    request.LastModified = request.DecidedAt ?? request.SubmittedAt;
                    await requestService.Add(request);
                    total++;
                }
                nextCarry[employee.Id] = CreditAllocationAdminController.CarryOver(free - used, policy.MaxCarryOver, allocation.MinBalance);
            }
            await allocationService.SaveChanges(token);
            await requestService.SaveChanges(token);
            carry = nextCarry;
        }
        logger.LogInformation("Seeded {Count} credit requests", total);
    }

    private static RequestStatus PickStatus(Faker faker, int year, DateTime created)
    {
        if (year < Today.Year)
            return faker.Random.WeightedRandom(
                new[] { RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Cancelled },
                new[] { .8f, .12f, .08f });
        var age = (Today - created).TotalDays;
        if (age < 21)
            return faker.Random.WeightedRandom(
                new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Approved },
                new[] { .35f, .45f, .2f });
        if (age < 60)
            return faker.Random.WeightedRandom(
                new[] { RequestStatus.Draft, RequestStatus.Submitted, RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Cancelled },
                new[] { .1f, .15f, .6f, .1f, .05f });
        return faker.Random.WeightedRandom(
            new[] { RequestStatus.Approved, RequestStatus.Rejected, RequestStatus.Cancelled },
            new[] { .8f, .12f, .08f });
    }

    private async Task SeedGroupTrainings(Faker faker, List<SeededEmployee> employees, Dictionary<int, string> departments, CancellationToken token)
    {
        var service = Service<GroupTraining>();
        var count = 0;
        foreach (var year in Years)
        {
            foreach (var template in SeedCatalog.GroupTrainings)
            {
                if (!faker.Random.Bool(0.85f)) continue;
                var start = faker.Date.Between(new DateTime(year, 1, 15), new DateTime(year, 12, 10));
                if (start.DayOfWeek == DayOfWeek.Saturday) start = start.AddDays(2);
                if (start.DayOfWeek == DayOfWeek.Sunday) start = start.AddDays(1);
                var startDate = DateOnly.FromDateTime(start);
                var inPast = start < Today;
                var status = inPast
                    ? faker.Random.WeightedRandom(new[] { GroupTrainingStatus.Completed, GroupTrainingStatus.Cancelled }, new[] { .92f, .08f })
                    : faker.Random.WeightedRandom(new[] { GroupTrainingStatus.Planned, GroupTrainingStatus.Cancelled }, new[] { .93f, .07f });

                var pool = employees
                    .Where(e => e.HireDate <= startDate && (e.IsActive || inPast))
                    .Where(e => template.Department == null || e.Department == template.Department)
                    .ToList();
                if (pool.Count == 0) continue;
                var participants = faker.PickRandom(pool, Math.Min(pool.Count, faker.Random.Int(6, 18))).ToList();

                await service.Add(new GroupTraining
                {
                    Title = template.Title,
                    Provider = template.Provider,
                    Description = template.Description,
                    Location = faker.PickRandom("Head office Ghent - training room", "Antwerp office", "Online (Teams)", "Provider premises, Brussels"),
                    StartDate = startDate,
                    DurationDays = template.Days,
                    TotalCost = template.CostPerPerson * participants.Count,
                    Status = status,
                    Created = start.AddDays(-faker.Random.Int(30, 90)).ToUniversalTime(),
                    Participants = participants.Select(p => new GroupTrainingParticipant
                    {
                        EmployeeId = p.Id,
                        Attended = status == GroupTrainingStatus.Completed && faker.Random.Bool(0.93f)
                    }).ToList()
                });
                count++;
            }
        }
        await service.SaveChanges(token);
        logger.LogInformation("Seeded {Count} group trainings", count);
    }

    private async Task SeedUsers(CancellationToken token)
    {
        foreach (var role in new[] { Roles.Admin, Roles.Employee })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var password = configuration["Seed:DemoPassword"] ?? throw new InvalidOperationException("Seed:DemoPassword is not configured");
        var users = new (string Email, string[] Roles)[]
        {
            (configuration["Seed:AdminEmail"] ?? "admin@qcredits.test", [Roles.Admin, Roles.Employee]),
            ("elise.maes@qcredits.test", [Roles.Employee]),
            ("tom.wouters@qcredits.test", [Roles.Employee]),
        };
        foreach (var (email, roles) in users)
        {
            var user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogWarning("Could not create {Email}: {Errors}", email, string.Join("; ", result.Errors.Select(e => e.Description)));
                continue;
            }
            await userManager.AddToRolesAsync(user, roles);
        }
    }

    private static string Slug(string value)
    {
        var normalized = value.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => char.IsAsciiLetter(c)).Select(char.ToLowerInvariant).ToArray();
        return new string(chars);
    }
}
