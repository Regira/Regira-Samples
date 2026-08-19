using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Balances;

/// <summary>
/// Computes an employee's QCredit balance for a given year:
/// Remaining = (AnnualCredits - ReservedCredits) + CarriedOverCredits - Sum(TotalCredits of Approved requests).
/// Reads the DbContext directly (read-only cross-entity aggregate, no single entity owns this data).
/// </summary>
public class BalanceCalculator(QCreditsDbContext db)
{
    public async Task<EmployeeBalanceDto?> Compute(int employeeId, int year, CancellationToken token = default)
    {
        var employee = await db.Employees.AsNoTracking().FirstOrDefaultAsync(x => x.Id == employeeId, token);
        if (employee == null)
        {
            return null;
        }

        var policy = await db.CreditPolicies.AsNoTracking().FirstOrDefaultAsync(x => x.Year == year, token);
        var carryOver = await db.EmployeeCarryOvers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeId == employeeId && x.Year == year, token);

        var approved = await db.QCreditRequests.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && x.Year == year && x.Status == RequestStatus.Approved)
            .SumAsync(x => (decimal?)x.TotalCredits, token) ?? 0m;
        var pending = await db.QCreditRequests.AsNoTracking()
            .Where(x => x.EmployeeId == employeeId && x.Year == year && x.Status == RequestStatus.Pending)
            .SumAsync(x => (decimal?)x.TotalCredits, token) ?? 0m;

        var annual = policy?.AnnualCredits ?? 20m;
        var reserved = policy?.ReservedCredits ?? 5m;
        var minBalance = policy?.MinBalance ?? -10m;
        var carried = carryOver?.CarriedOverCredits ?? 0m;

        return new EmployeeBalanceDto
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Department = employee.Department,
            Year = year,
            AnnualCredits = annual,
            ReservedCredits = reserved,
            FreelyAvailableCredits = annual - reserved,
            CarriedOverCredits = carried,
            ApprovedCredits = approved,
            PendingCredits = pending,
            RemainingCredits = annual - reserved + carried - approved,
            MinBalance = minBalance
        };
    }
}
