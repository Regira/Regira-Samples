using Microsoft.AspNetCore.Identity.UI.Services;

namespace QCredits.Api.Infrastructure.Security;

/// <summary>Development mail sender: logs the message (password-recovery links) instead of sending it.</summary>
public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation("Mail to {Email}: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
