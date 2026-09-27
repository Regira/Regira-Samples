using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Security.Authentication.Jwt.Extensions;

namespace EventPlanner.Api.Controllers;

/// <summary>Read-only employee directory (Identity users) — used to pick a participant and to show the signed-in profile.</summary>
[ApiController, Route("employees")]
public class EmployeesController(EventPlannerDbContext dbContext) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> Me(CancellationToken token)
    {
        var userId = User.FindUserId();
        var me = await Project(dbContext.Users.Where(u => u.Id == userId)).FirstOrDefaultAsync(token);
        return me == null ? NotFound() : Ok(me);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(string id, CancellationToken token)
    {
        var item = await Project(dbContext.Users.Where(u => u.Id == id)).FirstOrDefaultAsync(token);
        return item == null ? NotFound() : Ok(new { item });
    }

    /// <summary>List employees (entity-style envelope { items }). Same filters as search.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] EmployeeSearch so, CancellationToken token)
        => Ok(new { items = await Page(Filter(so), so).ToListAsync(token) });

    /// <summary>Search employees (entity-style envelope { items, count }).</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] EmployeeSearch so, CancellationToken token)
    {
        var query = Filter(so);
        var count = await query.CountAsync(token);
        return Ok(new { items = await Page(query, so).ToListAsync(token), count });
    }

    public record EmployeeSearch
    {
        public string? Q { get; init; }
        public string[]? Id { get; init; }
        public string[]? Ids { get; init; }
        public string[]? Exclude { get; init; }
        public string[]? Department { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }

    private IQueryable<AppUser> Filter(EmployeeSearch so)
    {
        var query = dbContext.Users.AsNoTracking();
        var ids = (so.Id ?? []).Concat(so.Ids ?? []).ToArray();
        if (ids.Length > 0) query = query.Where(u => ids.Contains(u.Id));
        if (so.Exclude is { Length: > 0 }) query = query.Where(u => !so.Exclude.Contains(u.Id));
        if (so.Department is { Length: > 0 }) query = query.Where(u => so.Department.Contains(u.Department!));
        // the SPA's autocomplete sends wildcard terms (*ann* *smith*): every term must match a name, email or department
        foreach (var term in (so.Q ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim('*')).Where(t => t.Length > 0))
        {
            var pattern = $"%{term}%";
            query = query.Where(u => EF.Functions.Like(u.FirstName!, pattern) || EF.Functions.Like(u.LastName!, pattern)
                                     || EF.Functions.Like(u.Email!, pattern) || EF.Functions.Like(u.Department!, pattern));
        }
        return query;
    }

    private static IQueryable<EmployeeDto> Page(IQueryable<AppUser> query, EmployeeSearch so)
    {
        var sorted = query.OrderBy(u => u.LastName).ThenBy(u => u.FirstName);
        // pageSize 0 = all (capped)
        var size = so.PageSize <= 0 ? 500 : Math.Min(so.PageSize, 500);
        return Project(sorted.Skip((Math.Max(so.Page, 1) - 1) * size).Take(size));
    }

    private static IQueryable<EmployeeDto> Project(IQueryable<AppUser> query) => query.Select(u => new EmployeeDto
    {
        Id = u.Id, FirstName = u.FirstName, LastName = u.LastName, Email = u.Email, Department = u.Department, JobTitle = u.JobTitle
    });
}
