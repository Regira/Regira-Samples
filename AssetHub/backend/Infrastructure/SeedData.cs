using AssetHub.Api.Entities.AssetAssignments;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using Bogus;
using Microsoft.EntityFrameworkCore;
using AssetHub.Api.Data;
using Regira.Entities.Services.Abstractions;

namespace AssetHub.Api.Infrastructure;

public static class SeedData
{
    public static async Task SeedAsync(IServiceProvider sp)
    {
        var dbContext = sp.GetRequiredService<AppDbContext>();
        // Already seeded (EnsureCreated only creates the schema once; the .db file is disposable in dev).
        if (await dbContext.Categories.AnyAsync())
        {
            return;
        }

        Randomizer.Seed = new Random(20260819);

        var categories = await SeedCategoriesAsync(sp);
        var statuses = await SeedAssetStatusesAsync(sp);
        var locations = await SeedLocationsAsync(sp);
        var suppliers = await SeedSuppliersAsync(sp);
        var employees = await SeedEmployeesAsync(sp);

        var assetIds = await SeedAssetsAsync(sp, categories, statuses, locations, suppliers);
        await SeedAssetAssignmentsAsync(sp, assetIds, employees);
    }

    private static async Task<List<int>> SeedCategoriesAsync(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Category, int>>();
        string[] names =
        [
            "Laptops", "Desktops", "Monitors", "Mobile Phones", "Tablets",
            "Networking Equipment", "Peripherals", "Tools & Machinery", "Office Furniture", "Audio/Video Equipment"
        ];
        foreach (var name in names)
        {
            await service.Add(new Category { Title = name, Description = $"{name} owned by the company" });
        }
        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.Categories);
    }

    private static async Task<List<int>> SeedAssetStatusesAsync(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<AssetStatus, int>>();
        (string Title, string Color, bool Operational, int Sort)[] statuses =
        [
            ("In Use", "#22c55e", true, 1),
            ("In Storage", "#0ea5e9", true, 2),
            ("On Order", "#a855f7", true, 3),
            ("In Repair", "#f59e0b", false, 4),
            ("Retired", "#64748b", false, 5),
            ("Lost / Stolen", "#ef4444", false, 6)
        ];
        foreach (var s in statuses)
        {
            await service.Add(new AssetStatus { Title = s.Title, ColorHex = s.Color, IsOperational = s.Operational, SortOrder = s.Sort });
        }
        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.AssetStatuses);
    }

    private static async Task<List<int>> SeedLocationsAsync(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Location, int>>();
        var faker = new Faker<Location>()
            .RuleFor(x => x.Title, f => $"{f.Address.City()} Office")
            .RuleFor(x => x.Building, f => f.PickRandom("HQ Building", "North Wing", "South Wing", "Annex", "Warehouse"))
            .RuleFor(x => x.Room, f => $"Floor {f.Random.Int(1, 6)} - Room {f.Random.Int(100, 499)}")
            .RuleFor(x => x.Address, f => f.Address.FullAddress());

        foreach (var location in faker.Generate(10))
        {
            await service.Add(location);
        }
        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.Locations);
    }

    private static async Task<List<int>> SeedSuppliersAsync(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Supplier, int>>();
        var faker = new Faker<Supplier>()
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.ContactName, f => f.Name.FullName())
            .RuleFor(x => x.Email, (f, s) => f.Internet.Email(s.ContactName))
            .RuleFor(x => x.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.Website, f => f.Internet.Url());

        foreach (var supplier in faker.Generate(14))
        {
            await service.Add(supplier);
        }
        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.Suppliers);
    }

    private static async Task<List<int>> SeedEmployeesAsync(IServiceProvider sp)
    {
        var service = sp.GetRequiredService<IEntityService<Employee, int>>();
        string[] departments = ["Engineering", "Sales", "Marketing", "Finance", "Human Resources", "Operations", "IT Support", "Customer Success"];
        var faker = new Faker<Employee>()
            .RuleFor(x => x.FirstName, f => f.Name.FirstName())
            .RuleFor(x => x.LastName, f => f.Name.LastName())
            .RuleFor(x => x.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName, "assethub-demo.local"))
            .RuleFor(x => x.Department, f => f.PickRandom(departments))
            .RuleFor(x => x.JobTitle, f => f.Name.JobTitle())
            .RuleFor(x => x.IsActive, f => f.Random.Bool(0.92f));

        foreach (var employee in faker.Generate(85))
        {
            await service.Add(employee);
        }
        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.Employees);
    }

    private static async Task<List<int>> SeedAssetsAsync(
        IServiceProvider sp, List<int> categoryIds, List<int> statusIds, List<int> locationIds, List<int> supplierIds)
    {
        var service = sp.GetRequiredService<IEntityService<Asset, int>>();

        string[][] modelsByIndex =
        [
            ["Dell Latitude 5440", "Lenovo ThinkPad T14", "Apple MacBook Pro 14\"", "HP EliteBook 840", "Microsoft Surface Laptop 6"],
            ["Dell OptiPlex 7020", "HP ProDesk 600", "Lenovo ThinkCentre M90", "Apple Mac Mini M2", "Custom Workstation Tower"],
            ["Dell UltraSharp U2724D", "LG 27UP850", "Samsung ViewFinity S8", "BenQ PD2725U", "AOC 24B2XH"],
            ["Apple iPhone 15", "Samsung Galaxy S24", "Google Pixel 8", "Apple iPhone 14", "Samsung Galaxy A55"],
            ["Apple iPad Pro 11\"", "Samsung Galaxy Tab S9", "Microsoft Surface Pro 10", "Lenovo Tab P12"],
            ["Cisco Catalyst 9200 Switch", "Ubiquiti UniFi AP", "Netgear ProSAFE Switch", "Fortinet FortiGate 60F", "TP-Link Access Point"],
            ["Logitech MX Master 3S", "Logitech MX Keys", "Wacom Intuos Pro", "Dell Wireless Keyboard/Mouse", "Jabra Evolve2 65 Headset"],
            ["Makita Cordless Drill Set", "Bosch Angle Grinder", "DeWalt Impact Driver", "Milwaukee Multi-Tool", "Stanley Tool Chest"],
            ["Herman Miller Aeron Chair", "IKEA Bekant Desk", "Steelcase Series 1 Chair", "Standing Desk Converter", "Filing Cabinet"],
            ["Sony PTZ Conference Camera", "Poly Studio X30 Bar", "Yamaha YVC-1000 Speakerphone", "Epson Projector EB-2250U"]
        ];

        var faker = new Faker();
        var ids = new List<int>();

        for (var i = 0; i < 500; i++)
        {
            var categoryIndex = faker.Random.Int(0, categoryIds.Count - 1);
            var models = modelsByIndex[categoryIndex % modelsByIndex.Length];
            var purchaseDate = faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow.AddDays(-3));

            var asset = new Asset
            {
                Title = faker.PickRandom(models),
                Description = faker.Lorem.Sentence(8),
                SerialNumber = $"{faker.Random.AlphaNumeric(4)}-{faker.Random.AlphaNumeric(8)}".ToUpperInvariant(),
                CategoryId = categoryIds[categoryIndex],
                StatusId = faker.PickRandom(statusIds),
                LocationId = faker.Random.Bool(0.85f) ? faker.PickRandom(locationIds) : null,
                SupplierId = faker.Random.Bool(0.8f) ? faker.PickRandom(supplierIds) : null,
                PurchaseDate = purchaseDate,
                PurchasePrice = Math.Round(faker.Random.Decimal(80, 3500), 2),
                Notes = faker.Random.Bool(0.3f) ? faker.Lorem.Sentence() : null
            };

            var attachmentCount = faker.Random.Int(0, 2);
            if (attachmentCount > 0)
            {
                asset.Attachments = Enumerable.Range(0, attachmentCount).Select(_ => new AssetAttachment
                {
                    FileName = faker.PickRandom("invoice.pdf", "manual.pdf", "photo.jpg", "spec-sheet.pdf", "warranty-card.pdf"),
                    ContentType = faker.PickRandom("application/pdf", "image/jpeg"),
                    SizeBytes = faker.Random.Long(20_000, 5_000_000),
                    Description = faker.Lorem.Sentence(4),
                    UploadedAt = faker.Date.Between(purchaseDate, DateTime.UtcNow)
                }).ToList();
            }

            if (faker.Random.Bool(0.6f))
            {
                var warrantyStart = purchaseDate;
                var warrantyYears = faker.Random.Int(1, 3);
                asset.Warranties =
                [
                    new AssetWarranty
                    {
                        Provider = faker.Company.CompanyName(),
                        WarrantyNumber = faker.Random.AlphaNumeric(10).ToUpperInvariant(),
                        StartDate = warrantyStart,
                        EndDate = warrantyStart.AddYears(warrantyYears),
                        Cost = faker.Random.Bool(0.5f) ? Math.Round(faker.Random.Decimal(20, 300), 2) : null,
                        CoverageDetails = "Parts and labor, next business day on-site support"
                    }
                ];
            }

            var maintenanceCount = faker.Random.Int(0, 3);
            if (maintenanceCount > 0)
            {
                asset.MaintenanceRecords = Enumerable.Range(0, maintenanceCount).Select(_ =>
                {
                    var maintenanceDate = faker.Date.Between(purchaseDate, DateTime.UtcNow);
                    return new AssetMaintenanceRecord
                    {
                        MaintenanceDate = maintenanceDate,
                        PerformedBy = faker.Name.FullName(),
                        Description = faker.PickRandom(
                            "Routine inspection and cleaning", "Firmware/OS update applied", "Battery replaced",
                            "Screen repaired", "Hardware diagnostics run", "Cable/port repair"),
                        Cost = faker.Random.Bool(0.6f) ? Math.Round(faker.Random.Decimal(10, 250), 2) : null,
                        NextDueDate = faker.Random.Bool(0.4f) ? maintenanceDate.AddMonths(faker.Random.Int(3, 12)) : null
                    };
                }).ToList();
            }

            await service.Add(asset);
        }

        await service.SaveChanges();

        return await ReadIdsAsync(sp, s => s.Assets);
    }

    private static async Task SeedAssetAssignmentsAsync(IServiceProvider sp, List<int> assetIds, List<int> employeeIds)
    {
        var service = sp.GetRequiredService<IEntityService<AssetAssignment, int>>();
        var faker = new Faker();

        foreach (var assetId in assetIds)
        {
            // ~70% of assets have some assignment history; of those, ~60% currently have an active holder.
            if (!faker.Random.Bool(0.7f))
            {
                continue;
            }

            var historyCount = faker.Random.Int(1, 3);
            var safeNow = DateTime.UtcNow.AddDays(-1);
            DateTime cursor = faker.Date.Between(DateTime.UtcNow.AddYears(-3), DateTime.UtcNow.AddMonths(-2));
            var hasActive = faker.Random.Bool(0.6f);

            for (var h = 0; h < historyCount; h++)
            {
                // No runway left for another closed (assigned -> returned) period -- stop this asset's history.
                if (cursor >= safeNow)
                {
                    break;
                }

                var isLast = h == historyCount - 1;
                var assignedDate = cursor;
                var employeeId = faker.PickRandom(employeeIds);

                if (isLast && hasActive)
                {
                    await service.Add(new AssetAssignment
                    {
                        AssetId = assetId,
                        EmployeeId = employeeId,
                        AssignedDate = assignedDate,
                        ReturnedDate = null,
                        Notes = faker.Random.Bool(0.25f) ? faker.Lorem.Sentence() : null
                    });
                }
                else
                {
                    var maxDuration = Math.Max(1, (int)(safeNow - assignedDate).TotalDays);
                    var duration = faker.Random.Int(1, Math.Min(300, maxDuration));
                    var returnedDate = assignedDate.AddDays(duration);

                    await service.Add(new AssetAssignment
                    {
                        AssetId = assetId,
                        EmployeeId = employeeId,
                        AssignedDate = assignedDate,
                        ReturnedDate = returnedDate,
                        Notes = faker.Random.Bool(0.2f) ? faker.Lorem.Sentence() : null
                    });
                    cursor = returnedDate.AddDays(faker.Random.Int(1, 30));
                }
            }
        }

        await service.SaveChanges();
    }

    private static async Task<List<int>> ReadIdsAsync<TEntity>(IServiceProvider sp, Func<AppDbContext, IQueryable<TEntity>> selector)
        where TEntity : class
    {
        var dbContext = sp.GetRequiredService<AppDbContext>();
        var ids = await selector(dbContext).AsNoTracking().Select(x => EF.Property<int>(x, "Id")).ToListAsync();
        return ids;
    }
}
