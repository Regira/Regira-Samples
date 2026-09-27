using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Regira.Security.Authentication.Jwt.Extensions;

namespace HelpDesk.Api.Infrastructure.Security;

/// <summary>
/// Everyone signed in may read; writes are gated per controller (allow-list keyed on the route value).
/// Deletes have their own, usually stricter, list. Controllers not listed keep their own [Authorize].
/// </summary>
public class WriteAuthorizationFilter : IAsyncActionFilter
{
    private static readonly string[] AdminOnly = [Roles.Admin];
    private static readonly string[] StaffOnly = [Roles.Admin, Roles.Agent];
    private static readonly string[] Everyone = Roles.All;

    private static readonly Dictionary<string, string[]> WriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Categories"] = AdminOnly,
        ["Priorities"] = AdminOnly,
        ["Statuses"] = AdminOnly,
        ["SupportTeams"] = AdminOnly,
        ["PersonAccounts"] = AdminOnly,
        ["Users"] = AdminOnly,
        ["Persons"] = StaffOnly,
        // customers create and edit their own tickets; the TicketPrepper restores the fields they may not touch
        ["Tickets"] = Everyone,
        ["TicketAttachments"] = Everyone,
        ["TicketComments"] = Everyone,
    };

    private static readonly Dictionary<string, string[]> DeleteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Tickets"] = StaffOnly,
        ["TicketAttachments"] = StaffOnly,
        ["Persons"] = AdminOnly,
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var route = (context.ActionDescriptor as ControllerActionDescriptor)?.AttributeRouteInfo?.Template ?? "";
        var method = context.HttpContext.Request.Method;

        // reads: every GET, plus the two POST query overloads
        var isRead = HttpMethods.IsGet(method)
            || route.EndsWith("/search", StringComparison.OrdinalIgnoreCase)
            || route.EndsWith("/list", StringComparison.OrdinalIgnoreCase);

        if (!isRead && !context.ActionDescriptor.EndpointMetadata.Any(m => m is IAllowAnonymous))
        {
            var table = HttpMethods.IsDelete(method) && DeleteRoles.ContainsKey(controller) ? DeleteRoles : WriteRoles;
            var userRoles = context.HttpContext.User.FindRoles();
            if (table.TryGetValue(controller, out var roles) && !roles.Intersect(userRoles, StringComparer.OrdinalIgnoreCase).Any())
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        await next();
    }
}
