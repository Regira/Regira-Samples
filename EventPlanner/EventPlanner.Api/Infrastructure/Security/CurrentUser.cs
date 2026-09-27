using Regira.Security.Authentication.Jwt.Extensions;

namespace EventPlanner.Api.Infrastructure.Security;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
}

/// <summary>
/// The identity a request (or a background job) acts as. Scoped: read from the HTTP principal, or set explicitly
/// by code that runs without a request (the seeder) through <see cref="RunAsSystem"/>.
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
    bool IsAdmin { get; }
    /// <summary>True for trusted server-side work (seeding); bypasses row scoping.</summary>
    bool IsSystem { get; }
    bool IsAuthenticated => IsSystem || UserId != null;
    bool CanManage => IsSystem || IsAdmin;
}

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private bool _isSystem;
    public string? UserId => _isSystem ? null : httpContextAccessor.HttpContext?.User.FindUserId();
    public bool IsAdmin => !_isSystem && (httpContextAccessor.HttpContext?.User.FindRoles()
        .Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase) ?? false);
    public bool IsSystem => _isSystem;

    /// <summary>Marks this scope as trusted system work. Only call from code that never serves a request.</summary>
    public void RunAsSystem() => _isSystem = true;
}
