using Bogus;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.InterventionTypes;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Suppliers;
using Fleet.Api.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace Fleet.Api.Data.Seeding;

/// <summary>
/// Seeds a realistic fleet through the IEntityService implementations (preppers, normalizers and reactors all run).
/// Everything is planned in memory first (so correlated values stay consistent), then saved in waves:
/// types -> suppliers (+capabilities) -> vehicles -> invoices -> interventions (+lines) -> invoice settle pass.
/// </summary>
public class FleetSeeder(
    FleetDbContext db,
    IEntityService<InterventionType, int> typeService,
    IEntityService<Supplier, int> supplierService,
    IEntityService<Vehicle, int> vehicleService,
    IEntityService<Invoice, int> invoiceService,
    IEntityService<Intervention, int> interventionService,
    ILogger<FleetSeeder> logger)
{
    private const int InterventionCount = 500;
    private const int VehicleCount = 160;

    // brand dealers mostly service their own make (keyed by instance: ids are assigned on save)
    private readonly Dictionary<Supplier, string> _dealerBrands = [];
    private readonly Faker _f = new("nl_BE") { Random = new Randomizer(20260927) };
    private readonly DateOnly _today = DateOnly.FromDateTime(DateTime.UtcNow);

    // (Code, Title, Category, IntervalKm, IntervalMonths, StandardCost, Description)
    private static readonly (string Code, string Title, InterventionCategory Cat, int? Km, int? Months, decimal Cost, string Desc)[] TypeCatalog =
    [
        ("OIL", "Oil & filter change", InterventionCategory.Maintenance, 15000, 12, 180m, "Engine oil and oil filter replacement."),
        ("SVC-S", "Minor service", InterventionCategory.Maintenance, 15000, 12, 260m, "Manufacturer minor service incl. fluids check."),
        ("SVC-L", "Major service", InterventionCategory.Maintenance, 60000, 24, 690m, "Manufacturer major service incl. spark plugs and filters."),
        ("FLT", "Air & cabin filter", InterventionCategory.Maintenance, 30000, 12, 75m, "Replacement of air and pollen filters."),
        ("BRK-F", "Brake fluid replacement", InterventionCategory.Maintenance, null, 24, 95m, "Brake fluid flush and bleed."),
        ("COOL", "Coolant flush", InterventionCategory.Maintenance, null, 36, 120m, "Cooling system flush and refill."),
        ("AIRC", "A/C service", InterventionCategory.Maintenance, null, 24, 140m, "Air conditioning refill and leak test."),
        ("TIM", "Timing belt replacement", InterventionCategory.Maintenance, 120000, 60, 850m, "Timing belt, tensioner and water pump."),
        ("BRK-P", "Brake pads & discs", InterventionCategory.Repair, null, null, 450m, "Replacement of worn brake pads and discs."),
        ("CLU", "Clutch replacement", InterventionCategory.Repair, null, null, 1200m, "Clutch kit and flywheel inspection."),
        ("EXH", "Exhaust repair", InterventionCategory.Repair, null, null, 380m, "Exhaust system / particulate filter repair."),
        ("SUSP", "Suspension repair", InterventionCategory.Repair, null, null, 620m, "Shock absorbers, springs or bushings."),
        ("ENG", "Engine diagnostics & repair", InterventionCategory.Repair, null, null, 950m, "Fault finding and engine repair."),
        ("GBX", "Gearbox repair", InterventionCategory.Repair, null, null, 1800m, "Gearbox overhaul or replacement."),
        ("MOT", "Periodic technical inspection", InterventionCategory.Inspection, null, 12, 65m, "Mandatory periodic roadworthiness inspection."),
        ("TACHO", "Tachograph calibration", InterventionCategory.Inspection, null, 24, 180m, "Legal tachograph check and calibration (heavy vehicles)."),
        ("TIR-S", "Seasonal tire swap", InterventionCategory.Tires, null, 6, 90m, "Summer/winter tire change incl. storage."),
        ("TIR-N", "Tire replacement (set)", InterventionCategory.Tires, 40000, null, 520m, "New set of tires, mounting and balancing."),
        ("ALN", "Wheel alignment", InterventionCategory.Tires, null, null, 85m, "Four-wheel alignment."),
        ("BODY", "Body damage repair", InterventionCategory.Bodywork, null, null, 1100m, "Panel repair and paintwork."),
        ("GLS", "Windscreen replacement", InterventionCategory.Bodywork, null, null, 420m, "Windscreen replacement incl. ADAS calibration."),
        ("BAT", "Battery replacement", InterventionCategory.Electrical, null, null, 190m, "12V battery replacement."),
        ("ELEC", "Electrical diagnostics", InterventionCategory.Electrical, null, null, 160m, "Diagnosis of electrical faults and warning lights."),
        ("EVB", "EV battery health check", InterventionCategory.Electrical, null, 12, 150m, "High-voltage battery state-of-health check."),
        ("CLN", "Interior & exterior valeting", InterventionCategory.Cleaning, null, 3, 60m, "Full valeting service."),
    ];

    // supplier profiles: name pattern -> capability codes
    private static readonly (string Kind, string[] Names, string[] Codes)[] SupplierProfiles =
    [
        ("dealer", ["Volkswagen Center", "Ford Garage", "Renault Dealer", "Mercedes-Benz Service", "Toyota Service", "Peugeot Garage", "BMW Service Point"],
            ["OIL", "SVC-S", "SVC-L", "FLT", "BRK-F", "COOL", "AIRC", "TIM", "BRK-P", "CLU", "EXH", "SUSP", "ENG", "GBX", "ALN", "BAT", "ELEC", "EVB"]),
        ("independent", ["Garage Peeters", "Autoservice Janssens", "Garage Maes", "Car Repair Willems", "Autotechniek Claes", "Garage Wouters"],
            ["OIL", "SVC-S", "FLT", "BRK-F", "COOL", "AIRC", "BRK-P", "CLU", "EXH", "SUSP", "ENG", "BAT", "ELEC", "TIR-S", "ALN"]),
        ("tires", ["Tyre Plus", "Banden Center", "Wheel & Tire Pro", "Pneu Express"],
            ["TIR-S", "TIR-N", "ALN", "BRK-P", "BAT"]),
        ("truck", ["Truck Service Center", "Heavy Duty Repairs", "TruckTech"],
            ["OIL", "SVC-S", "SVC-L", "FLT", "BRK-F", "COOL", "AIRC", "BRK-P", "CLU", "EXH", "SUSP", "ENG", "GBX", "TACHO", "TIR-N", "ALN", "BAT", "ELEC"]),
        ("body", ["Carrosserie Dubois", "Body Shop Vermeulen", "Carrosserie Lambert"],
            ["BODY", "GLS", "CLN"]),
        ("glass", ["Glass Repair Express", "Autoglas Service"],
            ["GLS"]),
        ("inspection", ["Technical Inspection Center", "Autokeuring"],
            ["MOT"]),
        ("ev", ["EV Care", "E-Mobility Service"],
            ["EVB", "ELEC", "BAT", "SVC-S", "FLT", "AIRC", "BRK-P", "TIR-S"]),
        ("cleaning", ["Clean Car Wash", "Valeting Pros"],
            ["CLN"]),
    ];

    private static readonly (string Make, string Model, VehicleType Type, FuelType[] Fuels, int Weight)[] VehicleCatalog =
    [
        ("Volkswagen", "Golf", VehicleType.Car, [FuelType.Petrol, FuelType.Diesel, FuelType.Hybrid], 8),
        ("Volkswagen", "ID.4", VehicleType.Car, [FuelType.Electric], 5),
        ("Skoda", "Octavia Combi", VehicleType.Car, [FuelType.Diesel, FuelType.Petrol, FuelType.Hybrid], 8),
        ("Toyota", "Corolla", VehicleType.Car, [FuelType.Hybrid], 6),
        ("Tesla", "Model 3", VehicleType.Car, [FuelType.Electric], 5),
        ("BMW", "3 Series Touring", VehicleType.Car, [FuelType.Diesel, FuelType.Hybrid], 4),
        ("Peugeot", "308 SW", VehicleType.Car, [FuelType.Petrol, FuelType.Diesel], 5),
        ("Renault", "Clio", VehicleType.Car, [FuelType.Petrol, FuelType.Lpg], 4),
        ("Ford", "Transit Custom", VehicleType.Van, [FuelType.Diesel], 8),
        ("Mercedes-Benz", "Sprinter", VehicleType.Van, [FuelType.Diesel], 7),
        ("Renault", "Kangoo", VehicleType.Van, [FuelType.Diesel, FuelType.Electric], 5),
        ("Volkswagen", "Crafter", VehicleType.Van, [FuelType.Diesel], 4),
        ("Volvo", "FH 500", VehicleType.Truck, [FuelType.Diesel], 4),
        ("DAF", "XF 480", VehicleType.Truck, [FuelType.Diesel], 4),
        ("Scania", "R 450", VehicleType.Truck, [FuelType.Diesel, FuelType.Cng], 3),
        ("Mercedes-Benz", "Citaro", VehicleType.Bus, [FuelType.Diesel, FuelType.Electric], 2),
        ("BMW", "R 1250 RT", VehicleType.Motorcycle, [FuelType.Petrol], 2),
        ("Krone", "Profi Liner", VehicleType.Trailer, [FuelType.None], 2),
    ];

    private static readonly string[] Departments = ["Sales", "Field Service", "Logistics", "Management", "Technical", "Facilities", "Distribution"];

    // (Codes, Weight, Priority weights low/normal/high/urgent, description templates)
    private static readonly (string[] Codes, int Weight, bool Repair, string Desc)[] Scenarios =
    [
        (["SVC-S"], 14, false, "Scheduled minor service"),
        (["OIL", "FLT"], 10, false, "Oil change and filters"),
        (["SVC-L", "BRK-F"], 6, false, "Scheduled major service"),
        (["SVC-L", "TIM"], 2, false, "Major service with timing belt"),
        (["AIRC"], 3, false, "A/C service before summer"),
        (["COOL"], 2, false, "Coolant replacement"),
        (["TIR-S"], 9, false, "Seasonal tire change"),
        (["TIR-N", "ALN"], 5, false, "Tires worn below legal limit"),
        (["BRK-P"], 6, true, "Brake pads and discs worn"),
        (["MOT"], 8, false, "Periodic technical inspection"),
        (["TACHO"], 2, false, "Tachograph calibration due"),
        (["BODY"], 4, true, "Body damage after minor collision"),
        (["GLS"], 3, true, "Cracked windscreen"),
        (["BAT"], 3, true, "Vehicle does not start - battery"),
        (["ELEC"], 3, true, "Warning light on dashboard"),
        (["EVB"], 3, false, "Annual EV battery health check"),
        (["CLN"], 5, false, "Full valeting"),
        (["SUSP"], 2, true, "Noise from suspension"),
        (["EXH"], 2, true, "Exhaust / DPF fault"),
        (["CLU"], 1, true, "Clutch slipping"),
        (["ENG"], 2, true, "Engine running rough"),
        (["GBX"], 1, true, "Gearbox issue"),
    ];

    public async Task Seed(CancellationToken token = default)
    {
        logger.LogInformation("Seeding fleet data...");

        // ---- wave 1: intervention types
        var types = TypeCatalog.Select(t => new InterventionType
        {
            Code = t.Code, Title = t.Title, Category = t.Cat, IntervalKm = t.Km, IntervalMonths = t.Months,
            StandardCost = t.Cost, Description = t.Desc, IsActive = true,
            Created = ToUtc(_today.AddYears(-3))
        }).ToList();
        foreach (var type in types) await typeService.Add(type, token);
        await typeService.SaveChanges(token);
        var typeByCode = types.ToDictionary(t => t.Code!);

        // ---- wave 2: suppliers + capabilities (owned join rows through the parent's navigation)
        var suppliers = new List<Supplier>();
        foreach (var profile in SupplierProfiles)
        {
            foreach (var name in profile.Names)
            {
                var city = _f.Address.City();
                var codes = profile.Codes.Where(_ => profile.Codes.Length <= 3 || _f.Random.Bool(0.9f)).ToList();
                if (profile.Kind == "dealer") codes.AddRange(["OIL", "SVC-S", "BRK-P"]);   // dealers always do the basics
                var title = profile.Kind is "dealer" or "tires" or "inspection" ? $"{name} {city}" : name;
                var supplier = new Supplier
                {
                    Title = Trim(title, 128),
                    VatNumber = $"BE0{_f.Random.Number(100_000_000, 999_999_999)}",
                    ContactPerson = Trim(_f.Name.FullName(), 128),
                    Email = $"service@{Slug(name)}.be",
                    Phone = $"+32 {_f.Random.Number(2, 9)} {_f.Random.Number(100, 999)} {_f.Random.Number(10, 99)} {_f.Random.Number(10, 99)}",
                    Street = Trim(_f.Address.StreetAddress(), 128),
                    PostalCode = _f.Random.Number(1000, 9990).ToString(),
                    City = Trim(city, 64),
                    Rating = _f.Random.Bool(0.85f) ? _f.Random.WeightedRandom([2, 3, 4, 5], [0.08f, 0.22f, 0.45f, 0.25f]) : null,
                    IsActive = _f.Random.Bool(0.93f),
                    Created = ToUtc(_today.AddDays(-_f.Random.Number(1100, 2200))),
                    InterventionTypes = codes.Distinct()
                        .Select(c => new SupplierInterventionType { InterventionTypeId = typeByCode[c].Id })
                        .ToList()
                };
                suppliers.Add(supplier);
                if (profile.Kind == "dealer") _dealerBrands[supplier] = name.Split(' ')[0];
            }
        }
        foreach (var supplier in suppliers) await supplierService.Add(supplier, token);
        await supplierService.SaveChanges(token);
        var capabilities = suppliers.ToDictionary(s => s.Id, s => s.InterventionTypes!.Select(x => x.InterventionTypeId).ToHashSet());

        // ---- plan vehicles (saved after the interventions are planned, so status/next service correlate)
        var vehicles = PlanVehicles();

        // ---- plan interventions
        var planned = PlanInterventions(vehicles, suppliers, typeByCode, capabilities);

        // vehicle status + next service derived from their intervention history
        for (var i = 0; i < vehicles.Count; i++)
        {
            var v = vehicles[i];
            var history = planned.Where(p => p.VehicleIndex == i).ToList();
            if (v.Status != VehicleStatus.Retired && history.Any(p => p.Item.Status == InterventionStatus.InProgress))
                v.Status = VehicleStatus.InMaintenance;
            if (v.Status == VehicleStatus.Retired)
            {
                v.NextServiceDate = null;
                continue;
            }
            var serviceTypeIds = new[] { "SVC-S", "SVC-L", "OIL" }.Select(c => typeByCode[c].Id).ToHashSet();
            var services = history.Where(p => p.Item.Lines!.Any(l => serviceTypeIds.Contains(l.InterventionTypeId))).ToList();
            var nextPlanned = services.Where(p => p.Item.Status == InterventionStatus.Planned)
                .Select(p => (DateOnly?)p.Item.ScheduledDate).Min();
            var lastDone = services.Where(p => p.Item.Status == InterventionStatus.Completed)
                .Select(p => (DateOnly?)p.Item.CompletedDate).Max();
            var next = nextPlanned ?? lastDone?.AddMonths(12) ?? _today.AddDays(_f.Random.Number(-25, 200));
            // most fleets book the service before it lapses; keep roughly a third of the lapsed ones overdue
            if (nextPlanned == null && next < _today && _f.Random.Bool(0.65f))
                next = _today.AddDays(_f.Random.Number(3, 120));
            else if (nextPlanned == null && next < _today.AddDays(-90))
                next = _today.AddDays(-_f.Random.Number(1, 90));   // a lapse is noticed within a quarter
            v.NextServiceDate = next;
        }

        // ---- wave 3: vehicles
        foreach (var v in vehicles) await vehicleService.Add(v, token);
        await vehicleService.SaveChanges(token);
        foreach (var p in planned) p.Item.VehicleId = vehicles[p.VehicleIndex].Id;

        // ---- wave 4: invoices (amounts settle once their interventions exist)
        var invoices = PlanInvoices(planned, suppliers);
        foreach (var invoice in invoices.Select(x => x.Invoice)) await invoiceService.Add(invoice, token);
        await invoiceService.SaveChanges(token);
        foreach (var (invoice, members) in invoices)
            foreach (var member in members) member.InvoiceId = invoice.Id;

        // ---- wave 5: interventions (+ lines via the parent's navigation); the invoice reactor settles totals
        foreach (var p in planned) await interventionService.Add(p.Item, token);
        await interventionService.SaveChanges(token);

        // ---- wave 6: settle pass - re-run the invoice prepper on every invoice (detached, no navigations)
        var storedInvoices = await db.Invoices.AsNoTracking().ToListAsync(token);
        foreach (var invoice in storedInvoices) await invoiceService.Modify(invoice, token);
        await invoiceService.SaveChanges(token);

        logger.LogInformation("Seeded {Types} intervention types, {Suppliers} suppliers, {Vehicles} vehicles, {Interventions} interventions, {Invoices} invoices",
            types.Count, suppliers.Count, vehicles.Count, planned.Count, invoices.Count);
    }

    private List<Vehicle> PlanVehicles()
    {
        var plates = new HashSet<string>();
        var vins = new HashSet<string>();
        var list = new List<Vehicle>();
        var weights = VehicleCatalog.Select(c => (float)c.Weight).ToArray();
        var total = weights.Sum();
        var probabilities = weights.Select(w => w / total).ToArray();
        while (list.Count < VehicleCount)
        {
            var model = _f.Random.WeightedRandom(VehicleCatalog, probabilities);
            var fuel = _f.PickRandom(model.Fuels);
            var year = model.Fuels.Contains(FuelType.Electric) && fuel == FuelType.Electric
                ? _f.Random.Number(2019, _today.Year)
                : _f.Random.Number(2014, _today.Year);
            var acquisition = new DateOnly(year, _f.Random.Number(1, 12), _f.Random.Number(1, 28));
            if (acquisition > _today) acquisition = _today.AddDays(-_f.Random.Number(10, 120));
            var ageDays = Math.Max(30, _today.DayNumber - acquisition.DayNumber);
            var kmPerYear = KmPerYear(model.Type);
            var mileage = (int)(ageDays / 365.0 * kmPerYear);
            var plate = NewPlate(plates, model.Type);
            var vin = NewVin(vins);
            var retired = year <= 2016 && _f.Random.Bool(0.35f);
            var status = retired ? VehicleStatus.Retired : _f.Random.Bool(0.04f) ? VehicleStatus.OutOfService : VehicleStatus.Active;
            list.Add(new Vehicle
            {
                LicensePlate = plate,
                Vin = vin,
                Make = model.Make,
                Model = model.Model,
                Year = year,
                VehicleType = model.Type,
                FuelType = fuel,
                Status = status,
                Mileage = mileage,
                Department = model.Type switch
                {
                    VehicleType.Truck or VehicleType.Trailer => "Logistics",
                    VehicleType.Bus => "Facilities",
                    VehicleType.Van => _f.PickRandom("Field Service", "Technical", "Distribution"),
                    _ => _f.PickRandom(Departments)
                },
                AssignedDriver = model.Type == VehicleType.Trailer || retired ? null : Trim(_f.Name.FullName(), 64),
                AcquisitionDate = acquisition,
                Notes = retired ? "Decommissioned - awaiting remarketing." : null,
                Created = ToUtc(acquisition)
            });
        }
        return list;
    }

    private sealed record PlannedIntervention(int VehicleIndex, Intervention Item);

    private List<PlannedIntervention> PlanInterventions(List<Vehicle> vehicles, List<Supplier> suppliers,
        Dictionary<string, InterventionType> typeByCode, Dictionary<int, HashSet<int>> capabilities)
    {
        var result = new List<PlannedIntervention>();
        var scenarioProbabilities = Normalize(Scenarios.Select(s => (float)s.Weight));
        // heavier-used vehicles get more interventions
        var vehicleWeights = Normalize(vehicles.Select(v => v.VehicleType switch
        {
            VehicleType.Truck or VehicleType.Bus => 2.2f,
            VehicleType.Van => 1.6f,
            VehicleType.Trailer or VehicleType.Motorcycle => 0.6f,
            _ => 1f
        } * (v.Status == VehicleStatus.Retired ? 0.4f : 1f)));
        var vehicleIndexes = Enumerable.Range(0, vehicles.Count).ToArray();

        var attempts = 0;
        while (result.Count < InterventionCount && attempts++ < InterventionCount * 20)
        {
            var vi = _f.Random.WeightedRandom(vehicleIndexes, vehicleWeights);
            var vehicle = vehicles[vi];
            var scenario = _f.Random.WeightedRandom(Scenarios, scenarioProbabilities);
            if (!Fits(scenario.Codes, vehicle)) continue;

            var typeIds = scenario.Codes.Select(c => typeByCode[c].Id).ToList();
            var capable = suppliers.Where(s => s.IsActive && typeIds.All(capabilities[s.Id].Contains)).ToList();
            // heavy vehicles go to truck specialists when one can do it
            if (vehicle.VehicleType is VehicleType.Truck or VehicleType.Bus or VehicleType.Trailer)
            {
                var heavy = capable.Where(s => capabilities[s.Id].Contains(typeByCode["TACHO"].Id)).ToList();
                if (heavy.Count > 0 && _f.Random.Bool(0.85f)) capable = heavy;
            }
            var ownBrand = capable.Where(s => !_dealerBrands.TryGetValue(s, out var brand) || brand == vehicle.Make).ToList();
            if (ownBrand.Count > 0) capable = ownBrand;
            if (capable.Count == 0) continue;
            var supplier = _f.PickRandom(capable);

            // date window: 24 months back to 60 days ahead, never before acquisition; retired vehicles only in the past year+
            var earliest = new[] { _today.AddMonths(-24), vehicle.AcquisitionDate!.Value.AddDays(14) }.Max();
            var latest = vehicle.Status == VehicleStatus.Retired ? _today.AddMonths(-10) : _today.AddDays(60);
            if (earliest >= latest) continue;
            var date = earliest.AddDays(_f.Random.Number(0, latest.DayNumber - earliest.DayNumber));
            if (date.DayOfWeek is DayOfWeek.Saturday) date = date.AddDays(2);
            if (date.DayOfWeek is DayOfWeek.Sunday) date = date.AddDays(1);

            var status = StatusFor(date);
            var priority = scenario.Repair
                ? _f.Random.WeightedRandom([InterventionPriority.Normal, InterventionPriority.High, InterventionPriority.Urgent], [0.35f, 0.45f, 0.20f])
                : _f.Random.WeightedRandom([InterventionPriority.Low, InterventionPriority.Normal, InterventionPriority.High], [0.30f, 0.62f, 0.08f]);
            var multiplier = CostMultiplier(vehicle.VehicleType);
            var kmPerDay = KmPerYear(vehicle.VehicleType) / 365.0;
            var mileageAtDate = Math.Max(0, vehicle.Mileage - (int)((_today.DayNumber - date.DayNumber) * kmPerDay));

            result.Add(new PlannedIntervention(vi, new Intervention
            {
                SupplierId = supplier.Id,
                Status = status,
                Priority = priority,
                ScheduledDate = date,
                CompletedDate = status == InterventionStatus.Completed
                    ? new[] { date.AddDays(_f.Random.WeightedRandom([0, 1, 2, 5], [0.55f, 0.25f, 0.12f, 0.08f])), _today }.Min()
                    : null,
                Mileage = status == InterventionStatus.Planned ? null : mileageAtDate,
                Description = status == InterventionStatus.Cancelled
                    ? $"{scenario.Desc} - cancelled: {_f.PickRandom("vehicle not available", "rescheduled with another supplier", "quote too high", "issue resolved under warranty")}"
                    : scenario.Desc,
                Created = ToUtc(new[] { date.AddDays(-_f.Random.Number(3, 30)), _today }.Min()),
                Lines = typeIds.Select(id =>
                {
                    var type = typeByCode.Values.First(t => t.Id == id);
                    return new InterventionLine
                    {
                        InterventionTypeId = id,
                        Cost = Math.Round(type.StandardCost * multiplier * (decimal)_f.Random.Double(0.85, 1.3), 2)
                    };
                }).ToList()
            }));
        }
        return result;
    }

    private List<(Invoice Invoice, List<Intervention> Members)> PlanInvoices(List<PlannedIntervention> planned, List<Supplier> suppliers)
    {
        var result = new List<(Invoice, List<Intervention>)>();
        var billable = planned.Select(p => p.Item)
            .Where(i => i.Status == InterventionStatus.Completed && i.CompletedDate <= _today.AddDays(-5))
            .Where(_ => _f.Random.Bool(0.9f))
            .GroupBy(i => i.SupplierId);
        foreach (var group in billable)
        {
            var supplier = suppliers.First(s => s.Id == group.Key);
            var prefix = _f.PickRandom("F", "INV", "FA", "");
            var seq = _f.Random.Number(100, 900);
            // monthly batches per supplier, split into invoices of 1-4 interventions
            foreach (var month in group.OrderBy(i => i.CompletedDate).GroupBy(i => new { i.CompletedDate!.Value.Year, i.CompletedDate!.Value.Month }))
            {
                var queue = new Queue<Intervention>(month);
                while (queue.Count > 0)
                {
                    var take = Math.Min(queue.Count, _f.Random.WeightedRandom([1, 2, 3, 4], [0.5f, 0.3f, 0.15f, 0.05f]));
                    var members = Enumerable.Range(0, take).Select(_ => queue.Dequeue()).ToList();
                    var lastDone = members.Max(m => m.CompletedDate!.Value);
                    var invoiceDate = new[] { lastDone.AddDays(_f.Random.Number(1, 12)), _today }.Min();
                    var age = _today.DayNumber - invoiceDate.DayNumber;
                    var status = age switch
                    {
                        > 60 => _f.Random.WeightedRandom([InvoiceStatus.Paid, InvoiceStatus.Disputed], [0.975f, 0.025f]),
                        > 30 => _f.Random.WeightedRandom([InvoiceStatus.Paid, InvoiceStatus.Approved, InvoiceStatus.Received, InvoiceStatus.Disputed], [0.8f, 0.1f, 0.05f, 0.05f]),
                        _ => _f.Random.WeightedRandom([InvoiceStatus.Received, InvoiceStatus.Approved, InvoiceStatus.Paid], [0.5f, 0.38f, 0.12f])
                    };
                    var paidDate = status == InvoiceStatus.Paid
                        ? new[] { invoiceDate.AddDays(_f.Random.Number(5, 35)), _today }.Min()
                        : (DateOnly?)null;
                    seq += _f.Random.Number(1, 7);
                    result.Add((new Invoice
                    {
                        InvoiceNumber = $"{prefix}{invoiceDate.Year}{(prefix.Length > 0 ? "-" : "")}{seq:00000}",
                        SupplierId = supplier.Id,
                        InvoiceDate = invoiceDate,
                        DueDate = invoiceDate.AddDays(30),
                        Status = status,
                        PaidDate = paidDate,
                        VatRate = 21m,
                        Notes = status == InvoiceStatus.Disputed ? "Labour hours higher than quoted - awaiting credit note." : null,
                        Created = ToUtc(invoiceDate)
                    }, members));
                }
            }
        }
        return result;
    }

    private InterventionStatus StatusFor(DateOnly date)
    {
        var days = date.DayNumber - _today.DayNumber;
        return days switch
        {
            > 0 => InterventionStatus.Planned,
            >= -6 => _f.Random.WeightedRandom([InterventionStatus.InProgress, InterventionStatus.Completed, InterventionStatus.Planned], [0.5f, 0.4f, 0.1f]),
            >= -45 => _f.Random.WeightedRandom([InterventionStatus.Completed, InterventionStatus.Cancelled, InterventionStatus.Planned, InterventionStatus.InProgress], [0.84f, 0.06f, 0.06f, 0.04f]),
            _ => _f.Random.WeightedRandom([InterventionStatus.Completed, InterventionStatus.Cancelled], [0.93f, 0.07f])
        };
    }

    private static bool Fits(string[] codes, Vehicle v)
    {
        var heavy = v.VehicleType is VehicleType.Truck or VehicleType.Bus;
        var electric = v.FuelType == FuelType.Electric;
        foreach (var code in codes)
        {
            if (code == "TACHO" && !heavy) return false;
            if (code == "EVB" && v.FuelType is not (FuelType.Electric or FuelType.Hybrid)) return false;
            if (electric && code is "OIL" or "SVC-L" or "TIM" or "EXH" or "CLU" or "GBX" or "COOL") return false;
            if (v.VehicleType == VehicleType.Trailer && code is not ("TIR-N" or "BRK-P" or "MOT" or "BODY" or "CLN" or "ALN" or "SUSP")) return false;
            if (v.VehicleType == VehicleType.Motorcycle && code is "TIM" or "AIRC" or "GLS" or "CLN" or "TACHO") return false;
            if (heavy && code is "TIR-S") return false;
        }
        return true;
    }

    private static int KmPerYear(VehicleType type) => type switch
    {
        VehicleType.Truck => 110_000,
        VehicleType.Bus => 65_000,
        VehicleType.Van => 32_000,
        VehicleType.Trailer => 80_000,
        VehicleType.Motorcycle => 9_000,
        _ => 24_000
    };

    private static decimal CostMultiplier(VehicleType type) => type switch
    {
        VehicleType.Truck => 2.3m,
        VehicleType.Bus => 2.5m,
        VehicleType.Van => 1.2m,
        VehicleType.Trailer => 0.9m,
        VehicleType.Motorcycle => 0.6m,
        _ => 1m
    };

    private string NewPlate(HashSet<string> used, VehicleType type)
    {
        while (true)
        {
            // Belgian format 1-ABC-234 (trucks/trailers often Q-/U- series)
            var first = type is VehicleType.Trailer ? "Q" : _f.Random.Number(1, 2).ToString();
            var plate = $"{first}-{_f.Random.String2(3, "ABCDEFGHJKLMNPRSTUVWXYZ")}-{_f.Random.Number(100, 999)}";
            if (used.Add(plate)) return plate;
        }
    }

    private string NewVin(HashSet<string> used)
    {
        while (true)
        {
            var vin = _f.Random.String2(17, "ABCDEFGHJKLMNPRSTUVWXYZ0123456789");
            if (used.Add(vin)) return vin;
        }
    }

    private static float[] Normalize(IEnumerable<float> weights)
    {
        var list = weights.ToArray();
        var total = list.Sum();
        return list.Select(w => w / total).ToArray();
    }

    private static DateTime ToUtc(DateOnly date) => date.ToDateTime(new TimeOnly(9, 0), DateTimeKind.Utc);
    private static string Trim(string value, int max) => value.Length <= max ? value : value[..max];
    private static string Slug(string value) => new string(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
}
