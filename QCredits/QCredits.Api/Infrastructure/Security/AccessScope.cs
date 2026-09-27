using Regira.Security.Authentication.Jwt.Extensions;

namespace QCredits.Api.Infrastructure.Security;

/// <summary>
/// Who is calling: an unrestricted caller (administrator or trusted writer) or an employee
/// who may only see their own rows (matched on e-mail address).
/// </summary>
public sealed class AccessScope(IHttpContextAccessor httpContextAccessor, WorkflowContext workflow)
{
    public bool IsTrustedWriter => workflow.IsTrustedWriter;
    public bool IsAdmin
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.Identity?.IsAuthenticated == true
                && user.FindRoles().Contains(Roles.Admin, StringComparer.OrdinalIgnoreCase);
        }
    }
    public bool IsUnrestricted => IsTrustedWriter || IsAdmin;

    /// <summary>Lower-cased e-mail of the signed-in user (null when anonymous).</summary>
    public string? Email
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true) return null;
            return (user.FindEmail() ?? user.FindUserName())?.Trim().ToLowerInvariant();
        }
    }
    public string? UserName => httpContextAccessor.HttpContext?.User.FindUserName() ?? Email;
}
