using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Regira.Security.Authentication.Jwt.Extensions;

namespace AssetHub.Api.Infrastructure.Security;

/// <summary>
/// Everyone signed in may read; only the listed roles may write (allow-list keyed on the controller route value).
/// </summary>
public class WriteAuthorizationFilter : IAsyncActionFilter
{
    private static readonly string[] AdminOnly = [Roles.Admin];
    private static readonly string[] Managers = [Roles.Admin, Roles.Manager];

    private static readonly Dictionary<string, string[]> WriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        // reference data: administrators only
        ["Categories"] = AdminOnly,
        ["AssetStatuses"] = AdminOnly,
        ["Locations"] = AdminOnly,
        ["Suppliers"] = AdminOnly,
        // inventory: administrators + asset managers
        ["Assets"] = Managers,
        ["AssetAttachments"] = Managers,
        ["AssetWorkflow"] = Managers,
        ["Employees"] = Managers,
    };

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"]?.ToString() ?? "";
        var route = (context.ActionDescriptor as ControllerActionDescriptor)?.AttributeRouteInfo?.Template ?? "";

        var isRead = HttpMethods.IsGet(context.HttpContext.Request.Method)
            || route.EndsWith("/search", StringComparison.OrdinalIgnoreCase)
            || route.EndsWith("/list", StringComparison.OrdinalIgnoreCase);

        var userRoles = context.HttpContext.User.FindRoles();

        if (!isRead
            && !context.ActionDescriptor.EndpointMetadata.Any(m => m is IAllowAnonymous)
            && WriteRoles.TryGetValue(controller, out var roles)
            && !roles.Intersect(userRoles, StringComparer.OrdinalIgnoreCase).Any())
        {
            context.Result = new ForbidResult();
            return;
        }

        await next();
    }
}
