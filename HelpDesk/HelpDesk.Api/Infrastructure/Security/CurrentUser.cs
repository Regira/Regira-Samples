using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Security.Authentication.Jwt.Extensions;

namespace HelpDesk.Api.Infrastructure.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Agent = "Agent";
    public const string Customer = "Customer";
    public const string Staff = Admin + "," + Agent;

    public static readonly string[] All = [Admin, Agent, Customer];
}

/// <summary>
/// The caller of the current scope. A request resolves it from the validated principal; a seeder or job has no
/// request and must opt in explicitly through <see cref="IsSystem"/> ("no HttpContext" is never "system").
/// </summary>
public class CurrentUser(IHttpContextAccessor httpContextAccessor, HelpDeskDbContext dbContext)
{
    private int? _personId;
    private bool _personResolved;

    public bool IsSystem { get; set; }

    public string? UserId => httpContextAccessor.HttpContext?.User.FindUserId();
    public IReadOnlyList<string> Roles => httpContextAccessor.HttpContext?.User.FindRoles() ?? [];

    public bool IsAdmin => IsSystem || Roles.Contains(Security.Roles.Admin, StringComparer.OrdinalIgnoreCase);
    public bool IsStaff => IsAdmin || Roles.Contains(Security.Roles.Agent, StringComparer.OrdinalIgnoreCase);
    /// <summary>A signed-in caller without a staff role: row-scoped to their own tickets.</summary>
    public bool IsCustomer => !IsStaff && UserId != null;

    /// <summary>The Person linked to the signed-in account (null for the system or an unlinked account).</summary>
    public async Task<int?> GetPersonId(CancellationToken token = default)
    {
        if (_personResolved) return _personId;
        var userId = UserId;
        _personId = userId == null
            ? null
            : await dbContext.Persons.AsNoTracking().Where(p => p.UserId == userId).Select(p => (int?)p.Id).FirstOrDefaultAsync(token);
        _personResolved = true;
        return _personId;
    }
}
