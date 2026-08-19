using Fleet.Api.Data;
using Fleet.Api.Entities.Interventions;
using Fleet.Api.Entities.Invoices;
using Fleet.Api.Entities.Vehicles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fleet.Api.Controllers;

// Cross-entity read-only aggregates -- no single entity owns these, so a plain ControllerBase
// queries the DbContext directly (see entities.patterns -> Cross-entity aggregates & report endpoints).
[ApiController, Route("dashboard")]
public class DashboardController(FleetDbContext db) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> Summary()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var vehicleCounts = await db.Vehicles.AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Active = g.Count(x => x.Status == VehicleStatus.Active),
                InMaintenance = g.Count(x => x.Status == VehicleStatus.InMaintenance),
                OutOfService = g.Count(x => x.Status == VehicleStatus.OutOfService)
            })
            .FirstOrDefaultAsync();

        var interventionCounts = await db.Interventions.AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new
            {
                Scheduled = g.Count(x => x.Status == InterventionStatus.Scheduled),
                InProgress = g.Count(x => x.Status == InterventionStatus.InProgress),
                Overdue = g.Count(x => x.Status != InterventionStatus.Completed && x.Status != InterventionStatus.Cancelled && x.ScheduledDate < now),
                CompletedThisMonth = g.Count(x => x.Status == InterventionStatus.Completed && x.CompletedDate != null && x.CompletedDate >= monthStart),
                TotalSpend = g.Sum(x => x.Cost),
                SpendThisMonth = g.Where(x => x.Created >= monthStart).Sum(x => x.Cost)
            })
            .FirstOrDefaultAsync();

        var supplierCounts = await db.Suppliers.AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new { Total = g.Count(), Active = g.Count(x => x.IsActive) })
            .FirstOrDefaultAsync();

        var invoiceCounts = await db.Invoices.AsNoTracking()
            .GroupBy(x => 1)
            .Select(g => new
            {
                Draft = g.Count(x => x.Status == InvoiceStatus.Draft),
                Overdue = g.Count(x => x.Status == InvoiceStatus.Overdue),
                Outstanding = g.Where(x => x.Status == InvoiceStatus.Sent || x.Status == InvoiceStatus.Overdue).Sum(x => x.TotalAmount)
            })
            .FirstOrDefaultAsync();

        var dto = new DashboardSummaryDto
        {
            TotalVehicles = vehicleCounts?.Total ?? 0,
            ActiveVehicles = vehicleCounts?.Active ?? 0,
            VehiclesInMaintenance = vehicleCounts?.InMaintenance ?? 0,
            OutOfServiceVehicles = vehicleCounts?.OutOfService ?? 0,

            ScheduledInterventions = interventionCounts?.Scheduled ?? 0,
            InProgressInterventions = interventionCounts?.InProgress ?? 0,
            OpenInterventions = (interventionCounts?.Scheduled ?? 0) + (interventionCounts?.InProgress ?? 0),
            OverdueInterventions = interventionCounts?.Overdue ?? 0,
            CompletedThisMonth = interventionCounts?.CompletedThisMonth ?? 0,
            TotalSpend = interventionCounts?.TotalSpend ?? 0m,
            SpendThisMonth = interventionCounts?.SpendThisMonth ?? 0m,

            TotalSuppliers = supplierCounts?.Total ?? 0,
            ActiveSuppliers = supplierCounts?.Active ?? 0,

            DraftInvoices = invoiceCounts?.Draft ?? 0,
            OverdueInvoices = invoiceCounts?.Overdue ?? 0,
            OutstandingAmount = invoiceCounts?.Outstanding ?? 0m
        };

        return Ok(dto);
    }

    [HttpGet("spend-by-month")]
    public async Task<ActionResult<IList<MonthlySpendDto>>> SpendByMonth([FromQuery] int year)
    {
        var raw = await db.Interventions.AsNoTracking()
            .Where(x => x.ScheduledDate.Year == year)
            .GroupBy(x => x.ScheduledDate.Month)
            .Select(g => new { Month = g.Key, Total = g.Sum(x => x.Cost) })
            .ToListAsync();

        var byMonth = raw.ToDictionary(x => x.Month, x => x.Total);
        var result = Enumerable.Range(1, 12)
            .Select(m => new MonthlySpendDto { Year = year, Month = m, Total = byMonth.GetValueOrDefault(m) })
            .ToList();

        return Ok(result);
    }

    [HttpGet("interventions-by-status")]
    public async Task<ActionResult<IList<StatusCountDto>>> InterventionsByStatus()
    {
        var raw = await db.Interventions.AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return Ok(raw.Select(x => new StatusCountDto { Status = x.Status.ToString(), Count = x.Count }).ToList());
    }

    [HttpGet("top-suppliers")]
    public async Task<ActionResult<IList<TopSupplierDto>>> TopSuppliers([FromQuery] int take = 5)
    {
        var raw = await db.Interventions.AsNoTracking()
            .GroupBy(x => new { x.SupplierId, x.Supplier!.Title })
            .Select(g => new { g.Key.SupplierId, g.Key.Title, Count = g.Count(), Total = g.Sum(x => x.Cost) })
            .OrderByDescending(x => x.Total)
            .Take(take)
            .ToListAsync();

        return Ok(raw.Select(x => new TopSupplierDto
        {
            SupplierId = x.SupplierId,
            SupplierName = x.Title,
            InterventionCount = x.Count,
            TotalSpend = x.Total
        }).ToList());
    }
}
