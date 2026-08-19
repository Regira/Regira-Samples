using Bogus;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace Fleet.Api.Data;

// Seeds sample data through IEntityService, in dependency-ordered waves (see entities.instructions
// -> Seeding via IEntityService / entities.patterns -> Multi-wave seeding). Idempotent: skips entirely
// once vehicles already exist, so app restarts against an existing fleet.db don't duplicate rows.
public static class FleetSeeder
{
    private const int VehicleCount = 150;
    private const int SupplierCount = 25;
    private const int InvoiceCount = 180;
    private const int InterventionCount = 500; // primary entity

    public static async Task SeedAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<FleetDbContext>();

        if (await db.Vehicles.AsNoTracking().AnyAsync())
        {
            return; // already seeded
        }

        var random = new Random(20260819);

        var interventionTypes = await SeedInterventionTypes(services);
        var vehicles = await SeedVehicles(services, random);
        var suppliers = await SeedSuppliers(services, interventionTypes, random);
        var invoices = await SeedInvoices(services, suppliers, random);
        await SeedInterventions(services, vehicles, suppliers, interventionTypes, invoices, random);

        // Second pass: Invoice.TotalAmount is aggregated from Interventions that point at it via the
        // optional InvoiceId FK (see InterventionInvoiceTotalPrepper). Rows created earlier in the same
        // wave can't see each other through that per-item prepper (nothing is persisted yet), so the
        // running total only reflects the last intervention processed per invoice. Recompute directly
        // once every intervention is on disk -- entities.patterns -> Aggregates over a non-owned child
        // collection calls this out explicitly ("seeding needs a second pass over the parents").
        await RecomputeInvoiceTotals(db);
    }

    private static async Task<List<InterventionType>> SeedInterventionTypes(IServiceProvider services)
    {
        var service = services.GetRequiredService<IEntityService<InterventionType, int>>();

        var definitions = new (string Title, string Description, decimal Cost, double Hours)[]
        {
            ("Oil Change", "Engine oil and filter replacement", 65, 0.5),
            ("Tire Rotation", "Rotate tires to even out tread wear", 35, 0.5),
            ("Brake Inspection", "Full inspection of brake pads, discs and lines", 45, 0.75),
            ("Brake Pad Replacement", "Replace worn front or rear brake pads", 180, 1.5),
            ("Battery Replacement", "Replace and test 12V starter battery", 150, 0.5),
            ("Engine Diagnostic", "Computer diagnostic scan of engine fault codes", 90, 1),
            ("Transmission Service", "Transmission fluid flush and filter service", 220, 2),
            ("Air Filter Replacement", "Replace engine and cabin air filters", 40, 0.5),
            ("Coolant Flush", "Drain and refill engine coolant system", 95, 1),
            ("Wheel Alignment", "Four-wheel alignment and steering check", 85, 1),
            ("Annual Safety Inspection", "Statutory annual roadworthiness inspection", 60, 1),
            ("Windshield Replacement", "Replace cracked or chipped windshield", 320, 2),
            ("AC Service", "Air conditioning recharge and leak check", 110, 1),
            ("Suspension Repair", "Repair or replace worn suspension components", 380, 3),
            ("Bodywork / Paint Repair", "Dent removal and touch-up paint work", 450, 4)
        };

        var items = definitions.Select(d => new InterventionType
        {
            Title = d.Title,
            Description = d.Description,
            EstimatedCost = d.Cost,
            EstimatedDurationHours = d.Hours,
            Created = DateTime.UtcNow.AddYears(-2)
        }).ToList();

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Vehicle>> SeedVehicles(IServiceProvider services, Random random)
    {
        var service = services.GetRequiredService<IEntityService<Vehicle, int>>();

        var fleet = new (string Brand, string Model, VehicleType Type)[]
        {
            ("Ford", "Focus", VehicleType.Car), ("Volkswagen", "Golf", VehicleType.Car),
            ("Toyota", "Corolla", VehicleType.Car), ("Opel", "Astra", VehicleType.Car),
            ("Skoda", "Octavia", VehicleType.Car), ("Renault", "Clio", VehicleType.Car),
            ("Ford", "Transit", VehicleType.Van), ("Volkswagen", "Caddy", VehicleType.Van),
            ("Mercedes-Benz", "Sprinter", VehicleType.Van), ("Renault", "Master", VehicleType.Van),
            ("Iveco", "Daily", VehicleType.Van), ("Peugeot", "Boxer", VehicleType.Van),
            ("Volvo", "FH16", VehicleType.Truck), ("Scania", "R450", VehicleType.Truck),
            ("MAN", "TGX", VehicleType.Truck), ("DAF", "XF", VehicleType.Truck),
            ("Krone", "Profi Liner", VehicleType.Trailer), ("Schmitz", "Cargobull", VehicleType.Trailer),
            ("Honda", "CB500", VehicleType.Motorcycle), ("Yamaha", "MT-07", VehicleType.Motorcycle)
        };

        var faker = new Faker("en");
        var plates = new HashSet<string>();
        var items = new List<Vehicle>();

        for (var i = 0; i < VehicleCount; i++)
        {
            var pick = fleet[random.Next(fleet.Length)];
            string plate;
            do { plate = $"{faker.Random.Replace("?-###-??")}".ToUpperInvariant(); } while (!plates.Add(plate));

            var status = RandomWeighted(random, [
                (VehicleStatus.Active, 75), (VehicleStatus.InMaintenance, 14),
                (VehicleStatus.OutOfService, 7), (VehicleStatus.Retired, 4)
            ]);
            var created = DateTime.UtcNow.AddDays(-random.Next(30, 365 * 4));

            items.Add(new Vehicle
            {
                LicensePlate = plate,
                Brand = pick.Brand,
                Model = pick.Model,
                Type = pick.Type,
                Status = status,
                Year = random.Next(2015, 2027),
                Mileage = random.Next(1_000, 260_000),
                Vin = faker.Vehicle.Vin(),
                LastServiceDate = random.Next(0, 10) > 1 ? DateTime.UtcNow.AddDays(-random.Next(5, 200)) : null,
                Created = created
            });
        }

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Supplier>> SeedSuppliers(IServiceProvider services, List<InterventionType> interventionTypes, Random random)
    {
        var service = services.GetRequiredService<IEntityService<Supplier, int>>();
        var faker = new Faker("en");

        var suffixes = new[] { "Garage", "Auto Service", "Motors", "Repair Center", "Fleet Care", "Workshop", "Tyre & Service" };
        var items = new List<Supplier>();

        for (var i = 0; i < SupplierCount; i++)
        {
            var name = $"{faker.Company.CompanyName()} {suffixes[random.Next(suffixes.Length)]}";
            var typeCount = random.Next(2, 7);
            var supportedTypes = interventionTypes.OrderBy(_ => random.Next()).Take(typeCount)
                .Select(t => new SupplierInterventionType { InterventionTypeId = t.Id })
                .ToList();

            items.Add(new Supplier
            {
                Title = name,
                ContactEmail = faker.Internet.Email(),
                ContactPhone = faker.Phone.PhoneNumber("+32 4## ## ## ##"),
                Address = $"{faker.Address.StreetAddress()}, {faker.Address.ZipCode()} {faker.Address.City()}",
                IsActive = random.Next(0, 10) > 0,
                SupportedInterventionTypes = supportedTypes,
                Created = DateTime.UtcNow.AddDays(-random.Next(60, 365 * 4))
            });
        }

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task<List<Invoice>> SeedInvoices(IServiceProvider services, List<Supplier> suppliers, Random random)
    {
        var service = services.GetRequiredService<IEntityService<Invoice, int>>();
        var items = new List<Invoice>();

        for (var i = 0; i < InvoiceCount; i++)
        {
            var supplier = suppliers[random.Next(suppliers.Count)];
            var issueDate = DateTime.UtcNow.AddDays(-random.Next(1, 365));
            var status = RandomWeighted(random, [
                (InvoiceStatus.Draft, 10), (InvoiceStatus.Sent, 25), (InvoiceStatus.Paid, 50),
                (InvoiceStatus.Overdue, 10), (InvoiceStatus.Cancelled, 5)
            ]);

            items.Add(new Invoice
            {
                SupplierId = supplier.Id,
                Status = status,
                IssueDate = issueDate,
                DueDate = issueDate.AddDays(30),
                Created = issueDate
            });
        }

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
        return items;
    }

    private static async Task SeedInterventions(
        IServiceProvider services,
        List<Vehicle> vehicles,
        List<Supplier> suppliers,
        List<InterventionType> interventionTypes,
        List<Invoice> invoices,
        Random random)
    {
        var service = services.GetRequiredService<IEntityService<Intervention, int>>();
        var faker = new Faker("en");

        var invoicesBySupplier = invoices
            .GroupBy(i => i.SupplierId)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Supplier objects returned from SeedSuppliers still carry the SupportedInterventionTypes we
        // set in memory (SaveChanges populates generated keys in place, it doesn't clear navigations) --
        // use that to bias supplier choice towards suppliers who actually support the chosen types.
        var suppliersByType = suppliers
            .SelectMany(s => (s.SupportedInterventionTypes ?? []).Select(sit => (TypeId: sit.InterventionTypeId, Supplier: s)))
            .GroupBy(x => x.TypeId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.Supplier).ToList());

        var items = new List<Intervention>();

        for (var i = 0; i < InterventionCount; i++)
        {
            var vehicle = vehicles[random.Next(vehicles.Count)];
            var typeCount = random.Next(1, 4);
            var chosenTypes = interventionTypes.OrderBy(_ => random.Next()).Take(typeCount).ToList();

            // Prefer a supplier that actually supports one of the chosen intervention types.
            var candidates = chosenTypes
                .SelectMany(t => suppliersByType.GetValueOrDefault(t.Id, []))
                .Distinct()
                .ToList();
            var supplier = candidates.Count > 0 ? candidates[random.Next(candidates.Count)] : suppliers[random.Next(suppliers.Count)];

            var status = RandomWeighted(random, [
                (InterventionStatus.Scheduled, 20), (InterventionStatus.InProgress, 10),
                (InterventionStatus.Completed, 60), (InterventionStatus.Cancelled, 10)
            ]);

            var scheduledDate = DateTime.UtcNow.AddDays(random.Next(-330, 30));
            DateTime? completedDate = status == InterventionStatus.Completed
                ? scheduledDate.AddDays(random.Next(0, 5))
                : null;

            var cost = chosenTypes.Sum(t => t.EstimatedCost) * (decimal)(0.85 + random.NextDouble() * 0.3);
            cost = Math.Round(cost, 2);

            int? invoiceId = null;
            if (status == InterventionStatus.Completed
                && invoicesBySupplier.TryGetValue(supplier.Id, out var supplierInvoices)
                && supplierInvoices.Count > 0
                && random.Next(0, 10) < 7)
            {
                invoiceId = supplierInvoices[random.Next(supplierInvoices.Count)].Id;
            }

            items.Add(new Intervention
            {
                VehicleId = vehicle.Id,
                SupplierId = supplier.Id,
                Status = status,
                ScheduledDate = scheduledDate,
                CompletedDate = completedDate,
                Notes = faker.Lorem.Sentence(random.Next(6, 16)),
                Cost = cost,
                InvoiceId = invoiceId,
                InterventionTypes = chosenTypes.Select(t => new InterventionInterventionType { InterventionTypeId = t.Id }).ToList(),
                Created = scheduledDate
            });
        }

        foreach (var item in items) await service.Add(item);
        await service.SaveChanges();
    }

    private static async Task RecomputeInvoiceTotals(FleetDbContext db)
    {
        var totals = await db.Interventions.AsNoTracking()
            .Where(i => i.InvoiceId != null)
            .GroupBy(i => i.InvoiceId!.Value)
            .Select(g => new { InvoiceId = g.Key, Total = g.Sum(i => i.Cost) })
            .ToListAsync();
        var totalsByInvoice = totals.ToDictionary(x => x.InvoiceId, x => x.Total);

        var invoices = await db.Invoices.ToListAsync();
        foreach (var invoice in invoices)
        {
            invoice.TotalAmount = totalsByInvoice.GetValueOrDefault(invoice.Id);
        }

        await db.SaveChangesAsync();
    }

    private static T RandomWeighted<T>(Random random, (T Value, int Weight)[] options)
    {
        var total = options.Sum(o => o.Weight);
        var roll = random.Next(0, total);
        var cumulative = 0;
        foreach (var (value, weight) in options)
        {
            cumulative += weight;
            if (roll < cumulative) return value;
        }
        return options[^1].Value;
    }
}
