using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Normalizing.Abstractions;
using Regira.Normalizing.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

/// <summary>
/// One ?q= covers code, subject and the customer (name, company, email): the customer text is folded into the
/// ticket's NormalizedContent instead of adding a second Q filter. Composes the whole value (never appends).
/// </summary>
public class TicketNormalizer(HelpDeskDbContext dbContext, INormalizer normalizer) : EntityNormalizerBase<Ticket>
{
    public override async Task HandleNormalize(Ticket item, CancellationToken token = default)
    {
        var customer = item.Customer != null
            ? new { item.Customer.GivenName, item.Customer.FamilyName, item.Customer.Company, item.Customer.Email }
            : await dbContext.Persons.AsNoTracking()
                .Where(p => p.Id == item.CustomerId)
                .Select(p => new { p.GivenName, p.FamilyName, p.Company, p.Email })
                .FirstOrDefaultAsync(token);

        item.NormalizedContent = normalizer.Normalize(string.Join(' ',
            new[] { item.Code, item.Title, customer?.GivenName, customer?.FamilyName, customer?.Company, customer?.Email }
                .Where(s => !string.IsNullOrWhiteSpace(s))));
    }
}
