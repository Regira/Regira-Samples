using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Balances;
using QCredits.Api.Data;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Controllers;

/// <summary>
/// Read-only cross-entity report endpoints (dashboard totals) - bypasses the entity pipeline and
/// queries the DbContext directly, per the Cross-entity aggregates & report endpoints pattern.
/// </summary>
[ApiController, Route("balances")]
public class BalancesController(QCreditsDbContext db, BalanceCalculator calculator) : ControllerBase
{
    [HttpGet("{employeeId:int}")]
    public async Task<IActionResult> GetForEmployee(int employeeId, [FromQuery] int? year, CancellationToken token)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var balance = await calculator.Compute(employeeId, y, token);
        if (balance == null)
        {
            return NotFound();
        }
        return Ok(balance);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? year, CancellationToken token)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var policy = await db.CreditPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Year == y, token);
        var annual = policy?.AnnualCredits ?? 20m;
        var reserved = policy?.ReservedCredits ?? 5m;
        var minBalance = policy?.MinBalance ?? -10m;

        var employees = await db.Employees.AsNoTracking().Where(x => x.IsActive).ToListAsync(token);
        var carryOvers = await db.EmployeeCarryOvers.AsNoTracking()
            .Where(x => x.Year == y)
            .ToDictionaryAsync(x => x.EmployeeId, x => x.CarriedOverCredits, token);
        var approvedByEmployee = await db.QCreditRequests.AsNoTracking()
            .Where(x => x.Year == y && x.Status == RequestStatus.Approved)
            .GroupBy(x => x.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Total = g.Sum(x => x.TotalCredits) })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Total, token);
        var pendingByEmployee = await db.QCreditRequests.AsNoTracking()
            .Where(x => x.Year == y && x.Status == RequestStatus.Pending)
            .GroupBy(x => x.EmployeeId)
            .Select(g => new { EmployeeId = g.Key, Total = g.Sum(x => x.TotalCredits) })
            .ToDictionaryAsync(x => x.EmployeeId, x => x.Total, token);

        var items = employees
            .Select(emp =>
            {
                var carried = carryOvers.GetValueOrDefault(emp.Id);
                var approved = approvedByEmployee.GetValueOrDefault(emp.Id);
                return new EmployeeBalanceDto
                {
                    EmployeeId = emp.Id,
                    EmployeeName = emp.FullName,
                    Department = emp.Department,
                    Year = y,
                    AnnualCredits = annual,
                    ReservedCredits = reserved,
                    FreelyAvailableCredits = annual - reserved,
                    CarriedOverCredits = carried,
                    ApprovedCredits = approved,
                    PendingCredits = pendingByEmployee.GetValueOrDefault(emp.Id),
                    RemainingCredits = annual - reserved + carried - approved,
                    MinBalance = minBalance
                };
            })
            .OrderBy(x => x.EmployeeName)
            .ToList();

        return Ok(new { items, count = items.Count });
    }
}
