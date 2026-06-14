using Bogus;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace Fleet.API.Data;

/// <summary>
/// Seeds a coherent sample data set (~500 interventions) through the Regira
/// <see cref="IEntityService{TEntity,TKey}"/> implementations, using Bogus for fake data.
/// </summary>
public static class FleetDbSeeder
{
    private const int VehicleCount = 40;
    private const int SupplierCount = 15;
    private const int InterventionCount = 500;

    private static readonly (string Code, string Title, int Minutes, decimal MinCost, decimal MaxCost)[] TypeCatalog =
    [
        ("OIL", "Oil & filter change", 45, 80, 180),
        ("BRAKE", "Brake inspection & pad replacement", 90, 150, 400),
        ("TYRE", "Tyre replacement", 60, 200, 800),
        ("INSP", "Periodic technical inspection", 60, 50, 120),
        ("BATT", "Battery replacement", 30, 100, 300),
        ("AC", "Air conditioning service", 60, 90, 250),
        ("TRANS", "Transmission service", 180, 250, 1200),
        ("ENGINE", "Engine diagnostics & repair", 240, 300, 3000),
        ("WASH", "Cleaning & detailing", 45, 30, 120),
        ("WIPER", "Wipers & fluids top-up", 15, 20, 60),
        ("SUSP", "Suspension repair", 150, 200, 1500),
        ("EXHAUST", "Exhaust system repair", 120, 150, 900)
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider, ILogger logger, CancellationToken token = default)
    {
        using var scope = serviceProvider.CreateScope();
        var sp = scope.ServiceProvider;
        var dbContext = sp.GetRequiredService<FleetDbContext>();

        if (await dbContext.Interventions.AnyAsync(token))
        {
            logger.LogInformation("Fleet database already contains data — skipping seeding.");
            return;
        }

        logger.LogInformation("Seeding Fleet sample data...");

        var typeService = sp.GetRequiredService<IEntityService<InterventionType, int>>();
        var vehicleService = sp.GetRequiredService<IEntityService<Vehicle, int>>();
        var supplierService = sp.GetRequiredService<IEntityService<Supplier, int>>();
        var invoiceService = sp.GetRequiredService<IEntityService<Invoice, int>>();
        var interventionService = sp.GetRequiredService<IEntityService<Intervention, int>>();

        Randomizer.Seed = new Random(20240614);
        var faker = new Faker("en");
        var now = DateTime.UtcNow;

        // ── 1. Intervention types ────────────────────────────────────────────
        var costRanges = new Dictionary<int, (decimal Min, decimal Max)>();
        var types = new List<InterventionType>();
        foreach (var def in TypeCatalog)
        {
            var type = new InterventionType
            {
                Code = def.Code,
                Title = def.Title,
                Description = faker.Lorem.Sentence(8),
                EstimatedDurationMinutes = def.Minutes
            };
            await typeService.Add(type, token);
            types.Add(type);
        }
        await typeService.SaveChanges(token); // assigns auto-increment Ids
        foreach (var (def, type) in TypeCatalog.Zip(types))
            costRanges[type.Id] = (def.MinCost, def.MaxCost);
        var typeIds = types.Select(t => t.Id).ToList();

        // ── 2. Suppliers (+ capabilities) ────────────────────────────────────
        var capableSuppliersByType = typeIds.ToDictionary(id => id, _ => new List<int>());
        var suppliers = new List<Supplier>();
        foreach (var _ in Enumerable.Range(0, SupplierCount))
        {
            var capabilityTypeIds = faker.PickRandom(typeIds, faker.Random.Int(3, 8)).Distinct().ToList();
            var supplier = new Supplier
            {
                Title = faker.Company.CompanyName() + " " + faker.PickRandom("Garage", "Motors", "Auto", "Service"),
                Email = faker.Internet.Email(),
                Phone = faker.Phone.PhoneNumber("+32 ## ### ## ##"),
                Address = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                VatNumber = "BE0" + faker.Random.Replace("#########"),
                Capabilities = capabilityTypeIds
                    .Select(tid => new SupplierInterventionType { InterventionTypeId = tid })
                    .ToList()
            };
            await supplierService.Add(supplier, token);
            suppliers.Add(supplier);
        }
        await supplierService.SaveChanges(token);
        // Map type -> capable supplier ids (using the choices we just made)
        for (var i = 0; i < suppliers.Count; i++)
            foreach (var cap in suppliers[i].Capabilities!)
                capableSuppliersByType[cap.InterventionTypeId].Add(suppliers[i].Id);

        // ── 3. Vehicles (+ allowed intervention types) ───────────────────────
        var vehicles = new List<Vehicle>();
        var allowedTypesByVehicle = new Dictionary<int, List<int>>();
        var plateCounter = 1;
        foreach (var _ in Enumerable.Range(0, VehicleCount))
        {
            var allowedTypeIds = faker.PickRandom(typeIds, faker.Random.Int(4, 10)).Distinct().ToList();
            var vehicle = new Vehicle
            {
                LicensePlate = $"{faker.Random.Int(1, 9)}-{faker.Random.String2(3, "ABCDEFGHIJKLMNOPQRSTUVWXYZ")}-{plateCounter++:D3}",
                Brand = faker.Vehicle.Manufacturer(),
                Model = faker.Vehicle.Model(),
                VehicleType = faker.PickRandomParam(
                    VehicleType.Car, VehicleType.Car, VehicleType.Van, VehicleType.Van,
                    VehicleType.Truck, VehicleType.Bus, VehicleType.Motorcycle, VehicleType.Trailer),
                Vin = faker.Vehicle.Vin(),
                Year = faker.Random.Int(2012, 2025),
                Mileage = faker.Random.Int(5_000, 320_000),
                AllowedInterventionTypes = allowedTypeIds
                    .Select(tid => new VehicleInterventionType { InterventionTypeId = tid })
                    .ToList()
            };
            await vehicleService.Add(vehicle, token);
            vehicles.Add(vehicle);
        }
        await vehicleService.SaveChanges(token);
        foreach (var v in vehicles)
            allowedTypesByVehicle[v.Id] = v.AllowedInterventionTypes!.Select(a => a.InterventionTypeId).ToList();

        // ── 4. Interventions (no invoice yet) ────────────────────────────────
        var interventions = new List<Intervention>(InterventionCount);
        foreach (var _ in Enumerable.Range(0, InterventionCount))
        {
            var vehicle = faker.PickRandom(vehicles);
            var allowed = allowedTypesByVehicle[vehicle.Id];
            var typeId = allowed.Count > 0 ? faker.PickRandom(allowed) : faker.PickRandom(typeIds);

            var capable = capableSuppliersByType[typeId];
            var supplierId = capable.Count > 0 ? faker.PickRandom(capable) : faker.PickRandom(suppliers).Id;

            var status = faker.PickRandomParam(
                InterventionStatus.Completed, InterventionStatus.Completed, InterventionStatus.Completed,
                InterventionStatus.Completed, InterventionStatus.Planned, InterventionStatus.InProgress,
                InterventionStatus.Cancelled);

            var scheduledDate = faker.Date.Between(now.AddYears(-2), now.AddDays(30));
            DateTime? completedDate = status == InterventionStatus.Completed
                ? scheduledDate.AddDays(faker.Random.Int(0, 5))
                : null;

            var (minCost, maxCost) = costRanges[typeId];
            var cost = status == InterventionStatus.Cancelled
                ? 0m
                : Math.Round(faker.Random.Decimal(minCost, maxCost), 2);

            var intervention = new Intervention
            {
                VehicleId = vehicle.Id,
                SupplierId = supplierId,
                InterventionTypeId = typeId,
                ScheduledDate = scheduledDate,
                CompletedDate = completedDate,
                Status = status,
                Cost = cost,
                MileageAtService = faker.Random.Int(5_000, vehicle.Mileage),
                Description = faker.Lorem.Sentence(faker.Random.Int(4, 12))
            };
            await interventionService.Add(intervention, token);
            interventions.Add(intervention);
        }
        await interventionService.SaveChanges(token);

        // ── 5. Invoices: bundle completed interventions per supplier ─────────
        var invoiceCounter = 1;
        foreach (var supplierGroup in interventions
                     .Where(x => x is { Status: InterventionStatus.Completed, CompletedDate: not null })
                     .GroupBy(x => x.SupplierId))
        {
            // Bill ~80% of a supplier's completed work, ordered chronologically.
            var billable = supplierGroup
                .OrderBy(x => x.CompletedDate)
                .Where(_ => faker.Random.Double() < 0.8)
                .ToList();

            foreach (var chunk in Chunk(billable, () => faker.Random.Int(2, 6)))
            {
                var issueDate = chunk.Max(x => x.CompletedDate!.Value).AddDays(faker.Random.Int(1, 7));
                var dueDate = issueDate.AddDays(30);
                var total = Math.Round(chunk.Sum(x => x.Cost), 2);
                var status = dueDate < now
                    ? faker.PickRandomParam(InvoiceStatus.Paid, InvoiceStatus.Paid, InvoiceStatus.Paid, InvoiceStatus.Overdue)
                    : faker.PickRandomParam(InvoiceStatus.Sent, InvoiceStatus.Sent, InvoiceStatus.Paid);

                var invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{issueDate:yyyy}-{invoiceCounter++:D5}",
                    SupplierId = supplierGroup.Key,
                    IssueDate = issueDate,
                    DueDate = dueDate,
                    Status = status,
                    TotalAmount = total
                };
                await invoiceService.Add(invoice, token);
                await invoiceService.SaveChanges(token); // need the invoice Id for the link

                foreach (var intervention in chunk)
                {
                    intervention.InvoiceId = invoice.Id;
                    await interventionService.Modify(intervention, token); // re-attach detached entity
                }
                await interventionService.SaveChanges(token);
            }
        }

        var invoiceTotal = await dbContext.Invoices.CountAsync(token);
        logger.LogInformation(
            "Seeded {Types} intervention types, {Suppliers} suppliers, {Vehicles} vehicles, {Interventions} interventions and {Invoices} invoices.",
            types.Count, suppliers.Count, vehicles.Count, interventions.Count, invoiceTotal);
    }

    /// <summary>Splits a list into consecutive chunks of randomly varying size.</summary>
    private static IEnumerable<List<T>> Chunk<T>(IReadOnlyList<T> source, Func<int> nextSize)
    {
        var index = 0;
        while (index < source.Count)
        {
            var size = Math.Max(1, nextSize());
            var chunk = source.Skip(index).Take(size).ToList();
            index += chunk.Count;
            yield return chunk;
        }
    }
}
