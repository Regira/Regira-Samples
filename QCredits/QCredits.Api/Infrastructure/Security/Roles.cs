namespace QCredits.Api.Infrastructure.Security;

public static class Roles
{
    /// <summary>HR administrator: manages allocations, policies, group trainings and approves requests.</summary>
    public const string Admin = "Admin";
    /// <summary>Regular employee: manages their own credit requests.</summary>
    public const string Employee = "Employee";
}
