using Fleet.Api.Data;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Api.Controllers;

/// <summary>
/// Read-only cross-entity aggregates for the fleet manager dashboard (bypasses the entity pipeline on purpose;
/// no row-level security exists in this app, so nothing has to be repeated here).
/// Decimal aggregates are done as REAL (SQLite has no decimal SUM/ORDER BY) and rounded afterwards.
/// </summary>
[ApiController, Route("dashboard")]
public class DashboardController(FleetDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var yearStart = new DateOnly(today.Year, 1, 1);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var twelveMonthsAgo = monthStart.AddMonths(-11);
        var serviceHorizon = today.AddDays(30);

        var vehiclesByStatus = await db.Vehicles.AsNoTracking()
            .GroupBy(v => v.Status).Select(g => new { Key = g.Key, Count = g.Count() }).ToListAsync(token);
        var vehiclesByType = await db.Vehicles.AsNoTracking().Where(v => v.Status != VehicleStatus.Retired)
            .GroupBy(v => v.VehicleType).Select(g => new { Key = g.Key, Count = g.Count() }).ToListAsync(token);
        var interventionsByStatus = await db.Interventions.AsNoTracking()
            .GroupBy(i => i.Status).Select(g => new { Key = g.Key, Count = g.Count() }).ToListAsync(token);

        var openStatuses = new[] { InterventionStatus.Planned, InterventionStatus.InProgress };
        var urgentOpen = await db.Interventions.AsNoTracking()
            .CountAsync(i => openStatuses.Contains(i.Status) && i.Priority == InterventionPriority.Urgent, token);
        var overduePlanned = await db.Interventions.AsNoTracking()
            .CountAsync(i => i.Status == InterventionStatus.Planned && i.ScheduledDate < today, token);
        var completedNotInvoiced = await db.Interventions.AsNoTracking()
            .CountAsync(i => i.Status == InterventionStatus.Completed && i.InvoiceId == null, token);

        var spendYtd = await db.Interventions.AsNoTracking()
            .Where(i => i.Status == InterventionStatus.Completed && i.CompletedDate >= yearStart)
            .SumAsync(i => (double)i.TotalCost, token);

        var spendByMonthRaw = await db.Interventions.AsNoTracking()
            .Where(i => i.Status == InterventionStatus.Completed && i.CompletedDate >= twelveMonthsAgo)
            .GroupBy(i => new { i.CompletedDate!.Value.Year, i.CompletedDate!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(i => (double)i.TotalCost), Count = g.Count() })
            .ToListAsync(token);
        var spendByMonth = Enumerable.Range(0, 12)
            .Select(offset => twelveMonthsAgo.AddMonths(offset))
            .Select(m =>
            {
                var hit = spendByMonthRaw.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);
                return new { Month = $"{m.Year:0000}-{m.Month:00}", Total = Math.Round(hit?.Total ?? 0, 2), Count = hit?.Count ?? 0 };
            })
            .ToList();

        var spendByCategory = await (
                from line in db.InterventionLines.AsNoTracking()
                join type in db.InterventionTypes.AsNoTracking() on line.InterventionTypeId equals type.Id
                join intervention in db.Interventions.AsNoTracking() on line.InterventionId equals intervention.Id
                where intervention.Status == InterventionStatus.Completed && intervention.CompletedDate >= twelveMonthsAgo
                group line by type.Category into g
                select new { Category = g.Key, Total = g.Sum(l => (double)l.Cost), Count = g.Count() })
            .ToListAsync(token);

        var topSuppliersRaw = await (
                from intervention in db.Interventions.AsNoTracking()
                join supplier in db.Suppliers.AsNoTracking() on intervention.SupplierId equals supplier.Id
                where intervention.Status == InterventionStatus.Completed && intervention.CompletedDate >= twelveMonthsAgo
                group intervention by new { supplier.Id, supplier.Title } into g
                select new { g.Key.Id, g.Key.Title, Total = g.Sum(i => (double)i.TotalCost), Count = g.Count() })
            .ToListAsync(token);
        var topSuppliers = topSuppliersRaw.OrderByDescending(x => x.Total).Take(6)
            .Select(x => new { x.Id, x.Title, Total = Math.Round(x.Total, 2), x.Count });

        var unpaid = await db.Invoices.AsNoTracking()
            .Where(i => i.Status != InvoiceStatus.Paid)
            .Select(i => new { i.Status, i.DueDate, Total = (double)i.TotalAmount })
            .ToListAsync(token);
        var invoicesByStatus = await db.Invoices.AsNoTracking()
            .GroupBy(i => i.Status).Select(g => new { Key = g.Key, Count = g.Count() }).ToListAsync(token);

        var upcomingServices = await db.Vehicles.AsNoTracking()
            .Where(v => v.Status != VehicleStatus.Retired && v.NextServiceDate != null && v.NextServiceDate <= serviceHorizon)
            .OrderBy(v => v.NextServiceDate)
            .Take(8)
            .Select(v => new { v.Id, v.LicensePlate, v.Make, v.Model, v.VehicleType, v.NextServiceDate, v.Mileage })
            .ToListAsync(token);
        var serviceDueCount = await db.Vehicles.AsNoTracking()
            .CountAsync(v => v.Status != VehicleStatus.Retired && v.NextServiceDate != null && v.NextServiceDate <= serviceHorizon, token);

        var activeVehicles = vehiclesByStatus.Where(x => x.Key != VehicleStatus.Retired).Sum(x => x.Count);
        var inMaintenance = vehiclesByStatus.Where(x => x.Key == VehicleStatus.InMaintenance).Sum(x => x.Count);

        return Ok(new
        {
            Kpis = new
            {
                FleetSize = activeVehicles,
                InMaintenance = inMaintenance,
                Availability = activeVehicles == 0 ? 0 : Math.Round(100.0 * (activeVehicles - inMaintenance
                    - vehiclesByStatus.Where(x => x.Key == VehicleStatus.OutOfService).Sum(x => x.Count)) / activeVehicles, 1),
                OpenInterventions = interventionsByStatus.Where(x => openStatuses.Contains(x.Key)).Sum(x => x.Count),
                UrgentOpen = urgentOpen,
                OverduePlanned = overduePlanned,
                CompletedNotInvoiced = completedNotInvoiced,
                SpendYtd = Math.Round(spendYtd, 2),
                OpenInvoiceAmount = Math.Round(unpaid.Sum(x => x.Total), 2),
                OpenInvoiceCount = unpaid.Count,
                OverdueInvoiceAmount = Math.Round(unpaid.Where(x => x.DueDate < today).Sum(x => x.Total), 2),
                OverdueInvoiceCount = unpaid.Count(x => x.DueDate < today),
                ServiceDueCount = serviceDueCount
            },
            VehiclesByStatus = vehiclesByStatus,
            VehiclesByType = vehiclesByType,
            InterventionsByStatus = interventionsByStatus,
            InvoicesByStatus = invoicesByStatus,
            SpendByMonth = spendByMonth,
            SpendByCategory = spendByCategory.OrderByDescending(x => x.Total)
                .Select(x => new { x.Category, Total = Math.Round(x.Total, 2), x.Count }),
            TopSuppliers = topSuppliers,
            UpcomingServices = upcomingServices
        });
    }
}
