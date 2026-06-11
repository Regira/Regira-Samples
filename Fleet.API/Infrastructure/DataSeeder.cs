using Bogus;
using Fleet.API.Data;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.Interventions;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Models;
using Regira.Entities.Services.Abstractions;

namespace Fleet.API.Infrastructure;

/// <summary>
/// Seeds the database with a realistic sample dataset (~500 interventions) using the
/// <see cref="IEntityService{T}"/> implementations, so preppers, normalizers and primers run exactly
/// as they would through the API. Bogus generates the fake data with a fixed seed for reproducibility.
/// </summary>
public static class DataSeeder
{
    private const int InterventionCount = 520;

    public static async Task SeedAsync(IServiceProvider sp, CancellationToken token = default)
    {
        var db = sp.GetRequiredService<FleetDbContext>();
        await db.Database.EnsureCreatedAsync(token);

        // Idempotent: only seed an empty database.
        if (await db.Interventions.AnyAsync(token))
            return;

        var typeService = sp.GetRequiredService<IEntityService<InterventionType>>();
        var supplierService = sp.GetRequiredService<IEntityService<Supplier>>();
        var vehicleService = sp.GetRequiredService<IEntityService<Vehicle, VehicleSearchObject, EntitySortBy, VehicleIncludes>>();
        var invoiceService = sp.GetRequiredService<IEntityService<Invoice>>();
        var interventionService = sp.GetRequiredService<IEntityService<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes>>();

        Randomizer.Seed = new Random(20240611);
        var faker = new Faker("en");

        var types = await SeedInterventionTypesAsync(typeService, token);
        var suppliers = await SeedSuppliersAsync(supplierService, faker, types, token);
        var vehicles = await SeedVehiclesAsync(vehicleService, faker, types, token);
        var invoicesBySupplier = await SeedInvoicesAsync(invoiceService, faker, suppliers, token);

        await SeedInterventionsAsync(interventionService, invoiceService, faker, types, suppliers, vehicles, invoicesBySupplier, token);
    }

    private static async Task<List<InterventionType>> SeedInterventionTypesAsync(
        IEntityService<InterventionType> service, CancellationToken token)
    {
        (string Code, string Title, int? IntervalKm, int Duration)[] definitions =
        [
            ("OIL", "Oil & filter change", 15000, 45),
            ("TIRE", "Tire replacement", 60000, 60),
            ("BRAKE", "Brake service", 40000, 90),
            ("INSPECT", "Periodic technical inspection", 20000, 120),
            ("BATTERY", "Battery replacement", null, 30),
            ("AIRCON", "Air-conditioning service", null, 60),
            ("ENGINE", "Engine repair", null, 240),
            ("TRANSM", "Transmission service", 100000, 180),
            ("BODY", "Bodywork & paint", null, 480),
            ("WINDOW", "Windshield replacement", null, 90),
            ("ELEC", "Electrical diagnostics", null, 120),
            ("EXHAUST", "Exhaust system repair", null, 120)
        ];

        var types = new List<InterventionType>();
        foreach (var (code, title, intervalKm, duration) in definitions)
        {
            var type = new InterventionType
            {
                Code = code,
                Title = title,
                Description = $"{title} performed according to manufacturer guidelines.",
                DefaultIntervalKm = intervalKm,
                EstimatedDurationMinutes = duration
            };
            await service.Add(type, token);
            types.Add(type);
        }
        await service.SaveChanges(token); // populates auto-increment Ids
        return types;
    }

    private static async Task<List<Supplier>> SeedSuppliersAsync(
        IEntityService<Supplier> service, Faker faker, List<InterventionType> types, CancellationToken token)
    {
        var suppliers = new List<Supplier>();
        for (var i = 0; i < 15; i++)
        {
            var capabilities = faker.PickRandom(types, faker.Random.Int(3, 8))
                .Select(t => new SupplierInterventionType { InterventionTypeId = t.Id })
                .ToList();

            var supplier = new Supplier
            {
                Title = faker.Company.CompanyName() + " " + faker.PickRandom("Garage", "Motors", "Service", "Truck Center"),
                ContactPerson = faker.Name.FullName(),
                Email = faker.Internet.Email(),
                Phone = faker.Phone.PhoneNumber("+32 ## ### ## ##"),
                Address = faker.Address.FullAddress(),
                Capabilities = capabilities
            };
            await service.Add(supplier, token);
            suppliers.Add(supplier);
        }
        await service.SaveChanges(token);
        return suppliers;
    }

    private static async Task<List<Vehicle>> SeedVehiclesAsync(
        IEntityService<Vehicle, VehicleSearchObject, EntitySortBy, VehicleIncludes> service,
        Faker faker, List<InterventionType> types, CancellationToken token)
    {
        var vehicles = new List<Vehicle>();
        for (var i = 0; i < 45; i++)
        {
            var allowed = faker.PickRandom(types, faker.Random.Int(4, 9))
                .Select(t => new VehicleInterventionType { InterventionTypeId = t.Id })
                .ToList();

            var vehicle = new Vehicle
            {
                LicensePlate = faker.Random.Replace("#-???-###").ToUpperInvariant(),
                Brand = faker.Vehicle.Manufacturer(),
                Model = faker.Vehicle.Model(),
                VehicleType = faker.PickRandomWithout(VehicleType.Trailer), // trailers added explicitly below
                Year = faker.Random.Int(2012, 2024),
                Mileage = faker.Random.Int(5_000, 320_000),
                Vin = faker.Vehicle.Vin(),
                AllowedInterventionTypes = allowed
            };
            await service.Add(vehicle, token);
            vehicles.Add(vehicle);
        }
        await service.SaveChanges(token);
        return vehicles;
    }

    private static async Task<Dictionary<int, List<Invoice>>> SeedInvoicesAsync(
        IEntityService<Invoice> service, Faker faker, List<Supplier> suppliers, CancellationToken token)
    {
        var invoicesBySupplier = new Dictionary<int, List<Invoice>>();
        var seq = 1000;
        foreach (var supplier in suppliers)
        {
            var list = new List<Invoice>();
            var count = faker.Random.Int(4, 7);
            for (var i = 0; i < count; i++)
            {
                var invoiceDate = faker.Date.Past(2);
                var status = faker.PickRandom<InvoiceStatus>();
                if (status == InvoiceStatus.Cancelled) status = InvoiceStatus.Sent; // keep most invoices billable
                var invoice = new Invoice
                {
                    Code = $"INV-{++seq}",
                    SupplierId = supplier.Id,
                    InvoiceDate = invoiceDate,
                    DueDate = invoiceDate.AddDays(30),
                    Status = status,
                    Amount = 0m // recomputed from linked interventions after seeding
                };
                await service.Add(invoice, token);
                list.Add(invoice);
            }
            invoicesBySupplier[supplier.Id] = list;
        }
        await service.SaveChanges(token);
        return invoicesBySupplier;
    }

    private static async Task SeedInterventionsAsync(
        IEntityService<Intervention, InterventionSearchObject, InterventionSortBy, InterventionIncludes> service,
        IEntityService<Invoice> invoiceService,
        Faker faker,
        List<InterventionType> types,
        List<Supplier> suppliers,
        List<Vehicle> vehicles,
        Dictionary<int, List<Invoice>> invoicesBySupplier,
        CancellationToken token)
    {
        // Base cost (EUR) per intervention type code.
        var baseCosts = new Dictionary<string, decimal>
        {
            ["OIL"] = 120, ["TIRE"] = 600, ["BRAKE"] = 400, ["INSPECT"] = 150,
            ["BATTERY"] = 200, ["AIRCON"] = 180, ["ENGINE"] = 2500, ["TRANSM"] = 1500,
            ["BODY"] = 1800, ["WINDOW"] = 450, ["ELEC"] = 220, ["EXHAUST"] = 500
        };

        // Which suppliers can perform a given intervention type.
        var capableSuppliersByType = types.ToDictionary(
            t => t.Id,
            t => suppliers.Where(s => s.Capabilities!.Any(c => c.InterventionTypeId == t.Id)).ToList());

        // Allowed type ids per vehicle.
        var allowedTypesByVehicle = vehicles.ToDictionary(
            v => v.Id,
            v => v.AllowedInterventionTypes!.Select(a => a.InterventionTypeId).ToList());

        var typeById = types.ToDictionary(t => t.Id);
        var created = new List<Intervention>();

        var seq = 0;
        var attempts = 0;
        while (created.Count < InterventionCount && attempts < InterventionCount * 5)
        {
            attempts++;
            var vehicle = faker.PickRandom(vehicles);

            // Candidate types: allowed for the vehicle AND performable by at least one supplier.
            var candidateTypeIds = allowedTypesByVehicle[vehicle.Id]
                .Where(id => capableSuppliersByType[id].Count > 0)
                .ToList();
            if (candidateTypeIds.Count == 0)
                continue;

            var typeId = faker.PickRandom(candidateTypeIds);
            var type = typeById[typeId];
            var supplier = faker.PickRandom(capableSuppliersByType[typeId]);

            var status = faker.Random.WeightedRandom(
                [InterventionStatus.Completed, InterventionStatus.Planned, InterventionStatus.InProgress, InterventionStatus.Cancelled],
                [0.70f, 0.12f, 0.10f, 0.08f]);

            var scheduledDate = faker.Date.Past(2);
            DateTime? completedDate = status == InterventionStatus.Completed
                ? scheduledDate.AddDays(faker.Random.Int(0, 5))
                : null;

            var multiplier = vehicle.VehicleType is VehicleType.Truck or VehicleType.Bus ? 1.6m : 1m;
            var cost = Math.Round(baseCosts[type.Code!] * multiplier * (decimal)faker.Random.Double(0.8, 1.35), 2);

            // Invoice completed interventions (most of them), choosing an invoice from the same supplier.
            int? invoiceId = null;
            if (status == InterventionStatus.Completed
                && invoicesBySupplier.TryGetValue(supplier.Id, out var supplierInvoices)
                && supplierInvoices.Count > 0
                && faker.Random.Double() < 0.85)
            {
                invoiceId = faker.PickRandom(supplierInvoices).Id;
            }

            var intervention = new Intervention
            {
                Code = $"INT-{++seq:00000}",
                VehicleId = vehicle.Id,
                InterventionTypeId = typeId,
                SupplierId = supplier.Id,
                InvoiceId = invoiceId,
                Status = status,
                ScheduledDate = scheduledDate,
                CompletedDate = completedDate,
                MileageAtService = faker.Random.Int(1_000, Math.Max(1_001, vehicle.Mileage)),
                Description = $"{type.Title} on {vehicle.Brand} {vehicle.Model} ({vehicle.LicensePlate}).",
                Cost = cost
            };
            await service.Add(intervention, token);
            created.Add(intervention);
        }
        await service.SaveChanges(token);

        // Recompute invoice totals from the interventions billed on them and persist via the service
        // (Modify re-tracks each invoice, as the repository clears the change tracker after SaveChanges).
        var totalsByInvoice = created
            .Where(x => x.InvoiceId.HasValue)
            .GroupBy(x => x.InvoiceId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Cost));

        var invoiceById = invoicesBySupplier.Values.SelectMany(x => x).ToDictionary(x => x.Id);
        foreach (var (invoiceId, total) in totalsByInvoice)
        {
            if (!invoiceById.TryGetValue(invoiceId, out var invoice))
                continue;
            invoice.Amount = total;
            await invoiceService.Modify(invoice, token);
        }
        await invoiceService.SaveChanges(token);
    }
}
