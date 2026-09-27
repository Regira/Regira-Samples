using System.Text;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using AssetHub.Api.Infrastructure.Security;
using AssetHub.Api.Services;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Services.Abstractions;

namespace AssetHub.Api.Data.Seeding;

/// <summary>
/// Seeds every entity through its IEntityService (preppers, primers, normalizers and the Related() sync run as usual).
/// Waves are flushed before their ids are used as foreign keys in the next wave.
/// </summary>
public static class DataSeeder
{
    private record Product(string Manufacturer, string Model, decimal MinPrice, decimal MaxPrice, int WarrantyMonths);

    private static readonly (string Title, string Icon, int Lifespan, string Description, Product[] Products)[] CatalogData =
    [
        ("Laptops", "laptop", 36, "Portable computers issued to staff",
        [
            new("Dell", "Latitude 5440", 950, 1450, 36), new("Lenovo", "ThinkPad T14 Gen 4", 1100, 1650, 36),
            new("HP", "EliteBook 840 G10", 1050, 1600, 36), new("Apple", "MacBook Pro 14 M3", 1900, 2700, 12),
            new("Apple", "MacBook Air 13 M2", 1150, 1500, 12), new("Lenovo", "ThinkPad X1 Carbon", 1600, 2300, 36)
        ]),
        ("Monitors", "display", 60, "External displays",
        [
            new("Dell", "P2723D 27\"", 280, 380, 36), new("LG", "27UL850 4K", 350, 480, 24),
            new("Samsung", "ViewFinity S8 32\"", 420, 560, 24), new("Dell", "U3423WE Ultrawide", 780, 990, 36)
        ]),
        ("Phones", "phone", 24, "Company mobile phones",
        [
            new("Apple", "iPhone 15", 850, 1050, 12), new("Samsung", "Galaxy S24", 780, 950, 24),
            new("Google", "Pixel 8", 650, 800, 24), new("Apple", "iPhone SE", 450, 550, 12)
        ]),
        ("Tablets", "tablet", 36, "Tablets for field work and meetings",
        [
            new("Apple", "iPad Air 11", 650, 850, 12), new("Samsung", "Galaxy Tab S9", 700, 900, 24),
            new("Microsoft", "Surface Pro 9", 1100, 1500, 24)
        ]),
        ("Desktops", "pc-display", 60, "Fixed workstations",
        [
            new("Dell", "OptiPlex 7010", 750, 1100, 36), new("HP", "Z2 Tower G9", 1400, 2200, 36),
            new("Lenovo", "ThinkCentre M90q", 800, 1150, 36)
        ]),
        ("Peripherals", "keyboard", 36, "Docks, headsets, keyboards and webcams",
        [
            new("Dell", "WD19S Dock", 180, 240, 36), new("Jabra", "Evolve2 65 Headset", 190, 260, 24),
            new("Logitech", "MX Keys Combo", 110, 160, 12), new("Logitech", "Brio 4K Webcam", 160, 210, 24)
        ]),
        ("Printers", "printer", 60, "Printers and scanners",
        [
            new("HP", "LaserJet Pro 4002dn", 280, 360, 12), new("Brother", "MFC-L8900CDW", 520, 680, 24),
            new("Zebra", "ZD421 Label Printer", 380, 470, 24)
        ]),
        ("Network", "router", 72, "Switches, access points and firewalls",
        [
            new("Cisco", "Catalyst 1000 Switch", 450, 900, 12), new("Ubiquiti", "U6 Pro Access Point", 140, 190, 12),
            new("Fortinet", "FortiGate 60F", 650, 900, 12)
        ]),
        ("Tools", "tools", 60, "Power tools and measuring equipment",
        [
            new("Bosch", "GSR 18V-60 Drill", 180, 260, 36), new("Makita", "DTD153 Impact Driver", 150, 210, 36),
            new("Fluke", "117 Multimeter", 220, 290, 36), new("Hilti", "TE 6-A22 Rotary Hammer", 480, 620, 24),
            new("Leica", "DISTO D2 Laser Meter", 140, 190, 24)
        ]),
        ("Projectors", "projector", 60, "Meeting room projectors",
        [
            new("Epson", "EB-L260F", 1100, 1400, 36), new("BenQ", "MW560", 420, 520, 36)
        ])
    ];

    private static readonly (string Title, string Color, StatusKind Kind, string Description)[] StatusData =
    [
        ("Available", "#198754", StatusKind.Available, "In stock and ready to be assigned"),
        ("In use", "#0d6efd", StatusKind.Assigned, "Assigned to an employee"),
        ("In repair", "#fd7e14", StatusKind.Maintenance, "Out for maintenance or repair"),
        ("On order", "#6f42c1", StatusKind.Inactive, "Ordered, not delivered yet"),
        ("Lost / stolen", "#dc3545", StatusKind.Inactive, "Reported lost or stolen"),
        ("Retired", "#6c757d", StatusKind.Inactive, "End of life, disposed or recycled")
    ];

    private static readonly (string Title, string Code, string City, string Country, string Address)[] LocationData =
    [
        ("Headquarters", "HQ", "Brussels", "Belgium", "Rue de la Loi 42"),
        ("Antwerp Office", "ANR", "Antwerp", "Belgium", "Meir 88"),
        ("Ghent Office", "GNT", "Ghent", "Belgium", "Korenmarkt 12"),
        ("Amsterdam Office", "AMS", "Amsterdam", "Netherlands", "Herengracht 301"),
        ("Paris Office", "PAR", "Paris", "France", "Rue de Rivoli 120"),
        ("Central Warehouse", "WH1", "Mechelen", "Belgium", "Industriepark 7"),
        ("Service Workshop", "WS", "Leuven", "Belgium", "Vaartkom 3"),
        ("Remote / Home office", "REM", "-", "-", "")
    ];

    private static readonly string[] Departments =
        ["Engineering", "Sales", "Marketing", "Finance", "HR", "IT", "Operations", "Support", "Legal", "Facilities"];

    public static async Task Seed(IServiceProvider sp, IConfiguration configuration, CancellationToken token = default)
    {
        await SeedUsers(sp, configuration);

        if (!configuration.GetValue("Seeding:Enabled", true)) return;
        var db = sp.GetRequiredService<AppDbContext>();
        if (await db.Categories.AnyAsync(token)) return;

        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DataSeeder));
        var assetCount = configuration.GetValue("Seeding:AssetCount", 500);
        // the seeder stamps historical assignments: it is a trusted writer of the workflow-owned fields
        sp.GetRequiredService<WorkflowContext>().IsTrustedWriter = true;

        Randomizer.Seed = new Random(20260927);
        var f = new Faker("en");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // ---- wave 1: reference data ----
        var categoryService = sp.GetRequiredService<IEntityService<Category, int>>();
        foreach (var c in CatalogData)
            await categoryService.Add(new Category { Title = c.Title, Icon = c.Icon, LifespanMonths = c.Lifespan, Description = c.Description }, token);
        await categoryService.SaveChanges(token);

        var statusService = sp.GetRequiredService<IEntityService<AssetStatus, int>>();
        for (var i = 0; i < StatusData.Length; i++)
        {
            var s = StatusData[i];
            await statusService.Add(new AssetStatus { Title = s.Title, Color = s.Color, Kind = s.Kind, Description = s.Description, SortOrder = i }, token);
        }
        await statusService.SaveChanges(token);

        var locationService = sp.GetRequiredService<IEntityService<Location, int>>();
        foreach (var l in LocationData)
            await locationService.Add(new Location { Title = l.Title, Code = l.Code, City = l.City, Country = l.Country, Address = l.Address }, token);
        await locationService.SaveChanges(token);

        var supplierService = sp.GetRequiredService<IEntityService<Supplier, int>>();
        var supplierNames = new HashSet<string>();
        while (supplierNames.Count < 24) supplierNames.Add(f.Company.CompanyName());
        foreach (var name in supplierNames)
        {
            var contact = f.Name.FullName();
            var domain = new string(name.ToLowerInvariant().Where(char.IsLetter).Take(14).ToArray()) + ".com";
            await supplierService.Add(new Supplier
            {
                Title = name,
                ContactName = contact,
                Email = $"sales@{domain}",
                Phone = f.Phone.PhoneNumber("+32 ## ### ## ##"),
                Website = $"https://www.{domain}",
                Address = $"{f.Address.StreetAddress()}, {f.Address.City()}",
                Notes = f.PickRandom("Preferred vendor", "Framework contract until 2027", "Net 30 payment terms", null, null)
            }, token);
        }
        await supplierService.SaveChanges(token);

        var categories = await db.Categories.AsNoTracking().OrderBy(x => x.Id).Select(x => new { x.Id, x.Title }).ToListAsync(token);
        var statuses = await db.AssetStatuses.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(token);
        var locationIds = await db.Locations.AsNoTracking().OrderBy(x => x.Id).Select(x => x.Id).ToListAsync(token);
        var supplierIds = await db.Suppliers.AsNoTracking().Select(x => x.Id).ToListAsync(token);

        // ---- wave 2: employees ----
        var employeeService = sp.GetRequiredService<IEntityService<Employee, int>>();
        var emails = new HashSet<string>();
        var employeeCount = Math.Max(40, assetCount * 3 / 10);
        for (var i = 1; i <= employeeCount; i++)
        {
            var first = f.Name.FirstName();
            var last = f.Name.LastName();
            var email = $"{first}.{last}".ToLowerInvariant().Replace("'", "").Replace(" ", "") + "@assethub.example";
            if (!emails.Add(email)) email = email.Replace("@", $"{i}@");
            emails.Add(email);
            await employeeService.Add(new Employee
            {
                Code = $"EMP-{i:D4}",
                FirstName = first,
                LastName = last,
                Email = email,
                Phone = f.Phone.PhoneNumber("+32 4## ## ## ##"),
                Department = f.PickRandom(Departments),
                JobTitle = f.Name.JobTitle(),
                LocationId = f.PickRandom(locationIds),
                IsActive = f.Random.Bool(0.9f),
                HireDate = today.AddDays(-f.Random.Int(30, 365 * 12))
            }, token);
        }
        await employeeService.SaveChanges(token);

        var employees = await db.Employees.AsNoTracking().Select(x => new { x.Id, x.IsActive, x.HireDate }).ToListAsync(token);
        var activeEmployeeIds = employees.Where(x => x.IsActive).Select(x => x.Id).ToList();
        var allEmployeeIds = employees.Select(x => x.Id).ToList();

        // ---- wave 3: assets with warranties, maintenance and assignment history (owned rows through the parent) ----
        var assetService = sp.GetRequiredService<IEntityService<Asset, int>>();
        var statusByKind = statuses.GroupBy(s => s.Kind).ToDictionary(g => g.Key, g => g.ToList());
        var availableStatus = statusByKind[StatusKind.Available][0];
        var assignedStatus = statusByKind[StatusKind.Assigned][0];
        var repairStatus = statusByKind[StatusKind.Maintenance][0];
        var inactiveStatuses = statusByKind[StatusKind.Inactive];
        var serials = new HashSet<string>();

        for (var i = 1; i <= assetCount; i++)
        {
            var catIndex = f.Random.WeightedRandom(
                Enumerable.Range(0, CatalogData.Length).ToArray(),
                [0.26f, 0.18f, 0.14f, 0.07f, 0.06f, 0.11f, 0.04f, 0.04f, 0.07f, 0.03f]);
            var cat = CatalogData[catIndex];
            var product = f.PickRandom(cat.Products);
            var purchaseDate = today.AddDays(-f.Random.Int(10, 365 * 6));
            var price = Math.Round(f.Random.Decimal(product.MinPrice, product.MaxPrice), 2);

            string serial;
            do serial = f.Random.Replace("??##########").ToUpperInvariant(); while (!serials.Add(serial));

            // status distribution: in use 55%, available 22%, repair 7%, inactive 16%
            var roll = f.Random.Double();
            var status = roll < 0.55 ? assignedStatus
                : roll < 0.77 ? availableStatus
                : roll < 0.84 ? repairStatus
                : f.PickRandom(inactiveStatuses);
            if (status.Title == "On order") purchaseDate = today.AddDays(-f.Random.Int(0, 20));

            var asset = new Asset
            {
                Code = $"AST-{i:D5}",
                Title = $"{product.Manufacturer} {product.Model}",
                Manufacturer = product.Manufacturer,
                Model = product.Model,
                SerialNumber = serial,
                Description = f.Random.Bool(0.35f) ? f.Lorem.Sentence(8) : null,
                CategoryId = categories[catIndex].Id,
                StatusId = status.Id,
                LocationId = f.Random.Bool(0.95f) ? f.PickRandom(locationIds) : null,
                SupplierId = f.PickRandom(supplierIds),
                PurchaseDate = purchaseDate,
                PurchasePrice = price,
                OrderNumber = $"PO-{purchaseDate.Year}-{f.Random.Int(1000, 9999)}",
                Warranties = [],
                MaintenanceRecords = [],
                Assignments = []
            };

            // warranties: the manufacturer's, sometimes an extension
            var warrantyEnd = purchaseDate.AddMonths(product.WarrantyMonths);
            asset.Warranties.Add(new Warranty
            {
                Type = WarrantyType.Manufacturer,
                Provider = product.Manufacturer,
                Reference = $"W-{f.Random.AlphaNumeric(8).ToUpperInvariant()}",
                StartDate = purchaseDate,
                EndDate = warrantyEnd,
                Coverage = "Parts and labour"
            });
            if (f.Random.Bool(0.3f))
            {
                asset.Warranties.Add(new Warranty
                {
                    Type = f.PickRandom(WarrantyType.Extended, WarrantyType.ServiceContract),
                    Provider = f.PickRandom(supplierNames.ToArray()),
                    Reference = $"EXT-{f.Random.AlphaNumeric(6).ToUpperInvariant()}",
                    StartDate = warrantyEnd,
                    EndDate = warrantyEnd.AddMonths(f.PickRandom(12, 24, 36)),
                    Cost = Math.Round(price * f.Random.Decimal(0.06m, 0.15m), 2),
                    Coverage = f.PickRandom("Next business day on-site", "Accidental damage", "Advance exchange", "Battery replacement")
                });
            }

            // maintenance: more for tools, printers and older kit
            var ageDays = today.DayNumber - purchaseDate.DayNumber;
            var maintenanceCount = ageDays < 60 ? 0 : f.Random.Int(0, catIndex is 6 or 8 ? 5 : 2);
            var maintenanceDates = Enumerable.Range(0, maintenanceCount)
                .Select(_ => purchaseDate.AddDays(f.Random.Int(30, Math.Max(31, ageDays))))
                .OrderBy(d => d).ToList();
            foreach (var date in maintenanceDates)
            {
                var type = f.PickRandom<MaintenanceType>();
                asset.MaintenanceRecords.Add(new MaintenanceRecord
                {
                    Type = type,
                    Date = date,
                    Title = type switch
                    {
                        MaintenanceType.Repair => f.PickRandom("Replaced battery", "Screen repair", "Keyboard replaced", "Motor brushes replaced", "Fan replaced"),
                        MaintenanceType.Inspection => f.PickRandom("Annual safety inspection", "Calibration check", "Electrical test (PAT)"),
                        MaintenanceType.Upgrade => f.PickRandom("RAM upgrade", "SSD upgrade", "Firmware update", "OS reinstall"),
                        MaintenanceType.Cleaning => "Deep clean",
                        _ => f.PickRandom("Scheduled service", "Preventive maintenance", "Consumables replaced")
                    },
                    Description = f.Random.Bool(0.5f) ? f.Lorem.Sentence(10) : null,
                    PerformedBy = f.PickRandom("IT Service Desk", product.Manufacturer + " Service", f.Name.FullName()),
                    Cost = type is MaintenanceType.Repair or MaintenanceType.Upgrade ? Math.Round(f.Random.Decimal(25, 350), 2) : f.Random.Bool(0.4f) ? Math.Round(f.Random.Decimal(0, 80), 2) : null,
                    NextDueDate = type is MaintenanceType.Inspection or MaintenanceType.Preventive ? date.AddMonths(12) : null
                });
            }

            // assignment history: sequential, non-overlapping periods since purchase
            if (status.Title != "On order" && ageDays > 14)
            {
                var historyCount = f.Random.Int(0, 3);
                var cursor = purchaseDate.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc).AddDays(f.Random.Int(1, 10));
                var end = DateTime.UtcNow.AddDays(-2);
                for (var h = 0; h < historyCount && cursor < end.AddDays(-20); h++)
                {
                    var returned = cursor.AddDays(f.Random.Int(20, Math.Max(21, (int)(end - cursor).TotalDays / 2)));
                    if (returned > end) break;
                    asset.Assignments.Add(new AssetAssignment
                    {
                        EmployeeId = f.PickRandom(allEmployeeIds),
                        AssignedOn = cursor,
                        ReturnedOn = returned,
                        Notes = f.Random.Bool(0.3f) ? f.PickRandom("New starter kit", "Temporary loan", "Replacement device", "Project equipment") : null,
                        ReturnNotes = f.Random.Bool(0.5f) ? f.PickRandom("Good condition", "Minor scratches", "Returned on departure", "Upgrade to newer model", "Charger missing") : null
                    });
                    cursor = returned.AddDays(f.Random.Int(1, 30));
                }
                if (status.Id == assignedStatus.Id)
                {
                    var assignedOn = cursor < end ? cursor : end.AddDays(-1);
                    var holder = f.PickRandom(activeEmployeeIds);
                    asset.Assignments.Add(new AssetAssignment
                    {
                        EmployeeId = holder,
                        AssignedOn = assignedOn,
                        Notes = f.Random.Bool(0.3f) ? f.PickRandom("New starter kit", "Replacement device", "Project equipment") : null
                    });
                    asset.CurrentEmployeeId = holder;
                    asset.AssignedOn = assignedOn;
                }
            }
            if (status.Id == assignedStatus.Id && asset.CurrentEmployeeId == null)
                asset.StatusId = availableStatus.Id;

            await assetService.Add(asset, token);
            if (i % 100 == 0) await assetService.SaveChanges(token);
        }
        await assetService.SaveChanges(token);

        // ---- wave 4: attachments (through the per-owner link service) ----
        var linkService = sp.GetRequiredService<IEntityService<AssetAttachment, int>>();
        var assetsForFiles = await db.Assets.AsNoTracking()
            .OrderBy(x => x.Id).Where(x => x.Id % 7 == 0)
            .Select(x => new { x.Id, x.Code, x.Title, x.SerialNumber, x.PurchaseDate, x.PurchasePrice, x.OrderNumber })
            .ToListAsync(token);
        foreach (var a in assetsForFiles)
        {
            var invoice = $"""
                INVOICE {a.OrderNumber}
                ------------------------------
                Item      : {a.Title}
                Serial    : {a.SerialNumber}
                Asset tag : {a.Code}
                Date      : {a.PurchaseDate:yyyy-MM-dd}
                Amount    : EUR {a.PurchasePrice:0.00}
                """;
            await linkService.Add(new AssetAttachment
            {
                ObjectId = a.Id,
                Attachment = new Attachment { FileName = $"invoice-{a.OrderNumber}.txt", ContentType = "text/plain", Bytes = Encoding.UTF8.GetBytes(invoice) }
            }, token);
            if (a.Id % 2 == 0)
            {
                var csv = $"property,value\nasset tag,{a.Code}\nmodel,{a.Title}\nserial,{a.SerialNumber}\n";
                await linkService.Add(new AssetAttachment
                {
                    ObjectId = a.Id,
                    Attachment = new Attachment { FileName = $"specs/{a.Code}-specs.csv", ContentType = "text/csv", Bytes = Encoding.UTF8.GetBytes(csv) }
                }, token);
            }
        }
        await linkService.SaveChanges(token);

        logger.LogInformation("Seeded {Categories} categories, {Statuses} statuses, {Locations} locations, {Suppliers} suppliers, {Employees} employees, {Assets} assets, {Attachments} attachment files",
            CatalogData.Length, StatusData.Length, LocationData.Length, supplierNames.Count, employeeCount, assetCount, await db.AssetAttachments.CountAsync(token));
    }

    private static async Task SeedUsers(IServiceProvider sp, IConfiguration configuration)
    {
        var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles.All)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var userManager = sp.GetRequiredService<UserManager<AppUser>>();
        foreach (var section in configuration.GetSection("Seeding:Users").GetChildren())
        {
            var email = section["Email"];
            var password = section["Password"];
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)) continue;
            if (await userManager.FindByNameAsync(email) != null) continue;

            var user = new AppUser
            {
                UserName = email, Email = email, EmailConfirmed = true,
                GivenName = section["GivenName"], FamilyName = section["FamilyName"]
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Seeding user {email} failed: {string.Join("; ", result.Errors.Select(e => e.Description))}");
            var roles = section.GetSection("Roles").Get<string[]>() ?? [];
            if (roles.Length > 0) await userManager.AddToRolesAsync(user, roles);
        }
    }
}
