using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;

namespace QCredits.Api.Services;

public record CreditUsage(double Used, double Pending);

public record CreditBalance(
    int EmployeeId, int Year,
    double AnnualCredits, double ReservedCredits, double ReservedUsed, double CarriedOver, double MinBalance,
    double FreeCredits, double UsedCredits, double PendingCredits, double RemainingCredits)
{
    /// <summary>Credits that can still be requested without going below the minimum balance (pending requests included).</summary>
    public double Requestable => RemainingCredits - PendingCredits - MinBalance;
}

/// <summary>
/// Computes QCredit balances. Only approved requests deduct credits; group trainings are funded
/// separately and never count. Free = Annual - Reserved + CarriedOver, Remaining = Free - Used.
/// Reads the DbContext directly: callers are responsible for row security.
/// </summary>
public class BalanceService(AppDbContext db)
{
    public static double Free(CreditAllocation a) => a.AnnualCredits - a.ReservedCredits + a.CarriedOver;

    public async Task<Dictionary<(int EmployeeId, int Year), CreditUsage>> GetUsage(
        IReadOnlyCollection<int> employeeIds, IReadOnlyCollection<int> years, CancellationToken token = default)
    {
        var rows = await db.CreditRequests.AsNoTracking()
            .Where(r => employeeIds.Contains(r.EmployeeId) && years.Contains(r.Year)
                && (r.Status == RequestStatus.Approved || r.Status == RequestStatus.Submitted))
            .GroupBy(r => new { r.EmployeeId, r.Year, r.Status })
            .Select(g => new { g.Key.EmployeeId, g.Key.Year, g.Key.Status, Total = g.Sum(r => r.TotalCredits) })
            .ToListAsync(token);

        return rows
            .GroupBy(r => (r.EmployeeId, r.Year))
            .ToDictionary(
                g => g.Key,
                g => new CreditUsage(
                    g.Where(r => r.Status == RequestStatus.Approved).Sum(r => r.Total),
                    g.Where(r => r.Status == RequestStatus.Submitted).Sum(r => r.Total)));
    }

    public void Apply(CreditAllocation allocation, CreditUsage? usage)
    {
        allocation.FreeCredits = Free(allocation);
        allocation.UsedCredits = usage?.Used ?? 0;
        allocation.PendingCredits = usage?.Pending ?? 0;
        allocation.RemainingCredits = allocation.FreeCredits - allocation.UsedCredits;
    }

    public async Task<CreditBalance?> GetBalance(int employeeId, int year, CancellationToken token = default)
    {
        var allocation = await db.CreditAllocations.AsNoTracking()
            .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Year == year, token);
        if (allocation == null) return null;
        var usage = await GetUsage([employeeId], [year], token);
        Apply(allocation, usage.GetValueOrDefault((employeeId, year)));
        return ToBalance(allocation);
    }

    public static CreditBalance ToBalance(CreditAllocation a) => new(
        a.EmployeeId, a.Year, a.AnnualCredits, a.ReservedCredits, a.ReservedUsed, a.CarriedOver, a.MinBalance,
        a.FreeCredits, a.UsedCredits, a.PendingCredits, a.RemainingCredits);
}
