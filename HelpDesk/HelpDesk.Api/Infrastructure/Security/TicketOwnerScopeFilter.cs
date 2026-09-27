using HelpDesk.Api.Entities.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Regira.Entities.Services.Abstractions;

namespace HelpDesk.Api.Infrastructure.Security;

/// <summary>
/// An attachment upload takes the owner id from the route and never runs a query, so no global filter sees it.
/// Re-run the ticket's scope for any action carrying an {objectId}: a ticket the caller cannot read answers 404,
/// the same as the read path.
/// </summary>
public class TicketOwnerScopeFilter(IEntityService<Ticket, int> tickets) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (context.RouteData.Values.TryGetValue("objectId", out var raw) && int.TryParse(raw?.ToString(), out var ticketId))
        {
            var ticket = await tickets.Details(ticketId);
            if (ticket == null)
            {
                context.Result = new NotFoundResult();
                return;
            }
        }
        await next();
    }
}
