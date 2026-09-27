using System.Security.Claims;
using EventPlanner.Api.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace EventPlanner.Api.Infrastructure.Security;

/// <summary>Adds the display name to the token so the SPA can greet the user without an extra call.</summary>
public class AppUserClaimsPrincipalFactory(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<AppUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        if (!string.IsNullOrWhiteSpace(user.FirstName)) identity.AddClaim(new Claim("given_name", user.FirstName));
        if (!string.IsNullOrWhiteSpace(user.LastName)) identity.AddClaim(new Claim("family_name", user.LastName));
        var displayName = string.Join(' ', new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (displayName.Length > 0) identity.AddClaim(new Claim("displayName", displayName));
        return identity;
    }
}

/// <summary>Development mail sender: logs the message (confirmation / password-reset links) instead of sending it.</summary>
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation("Mail to {Email}: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
