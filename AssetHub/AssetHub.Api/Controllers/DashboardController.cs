using AssetHub.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AssetHub.Api.Controllers;

/// <summary>
/// Read-only inventory overview. Bypasses the entity pipeline on purpose (aggregates);
/// the app has no row-level security, so there is no predicate to repeat here.
/// </summary>
[ApiController, Route("dashboard")]
public class DashboardController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var in60Days = today.AddDays(60);

        var totalAssets = await db.Assets.CountAsync(token);
        var assigned = await db.Assets.CountAsync(x => x.CurrentEmployeeId != null, token);
        // SQLite cannot SUM decimals server side -> sum client side over a narrow projection
        var prices = await db.Assets.Where(x => x.PurchasePrice != null).Select(x => x.PurchasePrice!.Value).ToListAsync(token);

        var byStatus = await db.AssetStatuses.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .Select(s => new { s.Id, s.Title, s.Color, s.Kind, Count = db.Assets.Count(a => a.StatusId == s.Id) })
            .ToListAsync(token);
        var byCategory = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new { c.Id, c.Title, c.Icon, Count = db.Assets.Count(a => a.CategoryId == c.Id) })
            .ToListAsync(token);
        var byLocation = await db.Locations.AsNoTracking()
            .OrderBy(l => l.Title)
            .Select(l => new { l.Id, l.Title, Count = db.Assets.Count(a => a.LocationId == l.Id) })
            .ToListAsync(token);

        var warrantiesExpiring = await db.Assets.AsNoTracking()
            .Where(a => a.Warranties!.Any(w => w.EndDate >= today && w.EndDate <= in60Days))
            .CountAsync(token);
        var maintenanceDue = await db.Assets.AsNoTracking()
            .Where(a => a.MaintenanceRecords!.Any(m => m.NextDueDate != null && m.NextDueDate <= today.AddDays(30)))
            .CountAsync(token);

        var recentAssignments = await db.AssetAssignments.AsNoTracking()
            .OrderByDescending(a => a.AssignedOn)
            .Take(8)
            .Select(a => new
            {
                a.Id, a.AssetId, AssetCode = a.Asset!.Code, AssetTitle = a.Asset.Title,
                a.EmployeeId, EmployeeName = a.Employee!.FirstName + " " + a.Employee.LastName,
                a.AssignedOn, a.ReturnedOn
            })
            .ToListAsync(token);

        return Ok(new
        {
            totalAssets,
            assigned,
            available = totalAssets - assigned,
            totalValue = prices.Sum(),
            employees = await db.Employees.CountAsync(x => x.IsActive, token),
            warrantiesExpiring,
            maintenanceDue,
            byStatus,
            byCategory,
            byLocation,
            recentAssignments
        });
    }
}
