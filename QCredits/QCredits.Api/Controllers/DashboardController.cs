using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Infrastructure.Security;
using QCredits.Api.Services;
using Regira.Security.Authentication.Jwt.Extensions;

namespace QCredits.Api.Controllers;

/// <summary>
/// Read-only aggregates. These queries bypass the entity pipeline, so the row security of the scope
/// filters is repeated here: an employee only sees figures about their own Employee record.
/// </summary>
[ApiController, Route("dashboard")]
public class DashboardController(AppDbContext db, AccessScope scope, BalanceService balances) : ControllerBase
{
    /// <summary>The signed-in user, the linked employee and the current-year balance.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var email = scope.Email;
        var employee = email == null ? null : await db.Employees.AsNoTracking().Include(x => x.Department)
            .Where(x => x.Email == email)
            .Select(x => new { x.Id, x.FirstName, x.LastName, x.Email, x.JobTitle, Department = x.Department!.Title })
            .FirstOrDefaultAsync();
        var balance = employee == null ? null : await balances.GetBalance(employee.Id, y);
        return Ok(new
        {
            email,
            roles = User.FindRoles(),
            isAdmin = scope.IsAdmin,
            year = y,
            employee,
            balance
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int? year)
    {
        var y = year ?? DateTime.UtcNow.Year;
        var email = scope.Email;
        var unrestricted = scope.IsUnrestricted;
        int? ownEmployeeId = unrestricted || email == null
            ? null
            : await db.Employees.Where(x => x.Email == email).Select(x => (int?)x.Id).FirstOrDefaultAsync();
        if (!unrestricted && ownEmployeeId == null) return Ok(new { year = y, scope = "none" });

        var allocationQuery = db.CreditAllocations.AsNoTracking().Where(x => x.Year == y);
        var requestQuery = db.CreditRequests.AsNoTracking().Where(x => x.Year == y);
        if (ownEmployeeId != null)
        {
            allocationQuery = allocationQuery.Where(x => x.EmployeeId == ownEmployeeId);
            requestQuery = requestQuery.Where(x => x.EmployeeId == ownEmployeeId);
        }

        var allocations = await allocationQuery
            .Select(x => new { x.EmployeeId, x.AnnualCredits, x.ReservedCredits, x.ReservedUsed, x.CarriedOver, x.MinBalance, Department = x.Employee!.Department!.Title })
            .ToListAsync();
        var requests = await requestQuery
            .Select(x => new { x.EmployeeId, x.Status, x.TotalCredits, x.TotalCost })
            .ToListAsync();

        var approved = requests.Where(r => r.Status == RequestStatus.Approved).ToList();
        var usedByEmployee = approved.GroupBy(r => r.EmployeeId).ToDictionary(g => g.Key, g => g.Sum(r => r.TotalCredits));
        var perEmployee = allocations.Select(a =>
        {
            var free = a.AnnualCredits - a.ReservedCredits + a.CarriedOver;
            var used = usedByEmployee.GetValueOrDefault(a.EmployeeId);
            return new { a.EmployeeId, a.Department, free, used, remaining = free - used, a.MinBalance };
        }).ToList();

        var itemQuery = db.CreditRequestItems.AsNoTracking()
            .Where(i => i.CreditRequest!.Year == y && i.CreditRequest.Status == RequestStatus.Approved);
        if (ownEmployeeId != null) itemQuery = itemQuery.Where(i => i.CreditRequest!.EmployeeId == ownEmployeeId);
        var items = await itemQuery.Select(i => new { i.Type, i.Credits, i.Cost }).ToListAsync();

        var from = new DateOnly(y, 1, 1);
        var to = from.AddYears(1);
        var trainingQuery = db.GroupTrainings.AsNoTracking()
            .Where(x => x.StartDate >= from && x.StartDate < to && x.Status != GroupTrainingStatus.Cancelled);
        if (ownEmployeeId != null) trainingQuery = trainingQuery.Where(x => x.Participants!.Any(p => p.EmployeeId == ownEmployeeId));
        var trainings = await trainingQuery.Select(x => new { x.TotalCost, x.ParticipantCount, x.DurationDays }).ToListAsync();

        var pendingQuery = db.CreditRequests.AsNoTracking().Where(x => x.Status == RequestStatus.Submitted);
        if (ownEmployeeId != null) pendingQuery = pendingQuery.Where(x => x.EmployeeId == ownEmployeeId);
        var pendingApprovals = await pendingQuery
            .OrderBy(x => x.SubmittedAt)
            .Take(8)
            .Select(x => new
            {
                x.Id, x.Title, x.Year, x.TotalCredits, x.SubmittedAt,
                Employee = x.Employee!.FirstName + " " + x.Employee.LastName,
                Department = x.Employee.Department!.Title
            })
            .ToListAsync();

        return Ok(new
        {
            year = y,
            scope = ownEmployeeId == null ? "organisation" : "personal",
            totals = new
            {
                employees = allocations.Count,
                annual = allocations.Sum(a => a.AnnualCredits),
                reserved = allocations.Sum(a => a.ReservedCredits),
                reservedUsed = allocations.Sum(a => a.ReservedUsed),
                carriedOver = allocations.Sum(a => a.CarriedOver),
                free = perEmployee.Sum(a => a.free),
                used = perEmployee.Sum(a => a.used),
                pending = requests.Where(r => r.Status == RequestStatus.Submitted).Sum(r => r.TotalCredits),
                remaining = perEmployee.Sum(a => a.remaining),
                approvedCost = approved.Sum(r => r.TotalCost),
                overdrawn = perEmployee.Count(a => a.remaining < 0),
                exhausted = perEmployee.Count(a => a.remaining <= 0)
            },
            requestsByStatus = Enum.GetValues<RequestStatus>()
                .Select(s => new { status = s, count = requests.Count(r => r.Status == s), credits = requests.Where(r => r.Status == s).Sum(r => r.TotalCredits) }),
            usageByType = Enum.GetValues<ActivityType>()
                .Select(t => new { type = t, credits = items.Where(i => i.Type == t).Sum(i => i.Credits), cost = items.Where(i => i.Type == t).Sum(i => i.Cost) })
                .Where(x => x.credits > 0)
                .OrderByDescending(x => x.credits),
            usageByDepartment = perEmployee
                .GroupBy(a => a.Department)
                .Select(g => new { department = g.Key, employees = g.Count(), free = g.Sum(a => a.free), used = g.Sum(a => a.used) })
                .OrderBy(x => x.department),
            groupTrainings = new
            {
                count = trainings.Count,
                totalCost = trainings.Sum(t => t.TotalCost),
                participants = trainings.Sum(t => t.ParticipantCount),
                days = trainings.Sum(t => t.DurationDays)
            },
            pendingApprovals
        });
    }
}
