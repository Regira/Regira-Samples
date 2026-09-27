using Microsoft.AspNetCore.Identity.UI.Services;

namespace AssetHub.Api.Infrastructure.Security;

/// <summary>Development mailer: logs the message (with its links) instead of sending it</summary>
public class DevEmailSender(ILogger<DevEmailSender> logger) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        logger.LogInformation("[DEV MAIL] To: {Email} | Subject: {Subject}\n{Body}", email, subject, htmlMessage);
        return Task.CompletedTask;
    }
}
