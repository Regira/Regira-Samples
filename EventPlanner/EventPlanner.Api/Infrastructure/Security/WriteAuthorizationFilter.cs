using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Regira.Security.Authentication.Jwt.Extensions;

namespace EventPlanner.Api.Infrastructure.Security;

/// <summary>
/// Everyone signed in may read; only administrators may write the reference data and events.
/// Allow-list keyed on the controller route value (class name minus "Controller").
/// Registrations are not listed: employees write their own (row-scoped by RegistrationScopeQueryBuilder).
/// </summary>
public class WriteAuthorizationFilter : IAsyncActionFilter
{
    private static readonly Dictionary<string, string[]> WriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Locations"] = [Roles.Admin],
        ["Speakers"] = [Roles.Admin],
        ["EventCategories"] = [Roles.Admin],
        ["Events"] = [Roles.Admin],
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
