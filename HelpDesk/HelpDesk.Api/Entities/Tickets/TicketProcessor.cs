using HelpDesk.Api.Data;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Processing.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

/// <summary>
/// Fills the list-card counters (comments, attachments) and hides internal notes from customers.
/// Safe to strip comments here: the ticket input DTO never carries them, so no write can sync the stripped list back.
/// </summary>
public class TicketProcessor(HelpDeskDbContext dbContext, CurrentUser currentUser) : IEntityProcessor<Ticket, TicketIncludes>
{
    public async Task Process(IList<Ticket> items, TicketIncludes? includes, CancellationToken token = default)
    {
        if (items.Count == 0) return;
        var ids = items.Select(x => x.Id).ToList();
        var staff = currentUser.IsStaff;

        var commentCounts = await dbContext.TicketComments.AsNoTracking()
            .Where(c => ids.Contains(c.TicketId) && (staff || !c.IsInternal))
            .GroupBy(c => c.TicketId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, token);
        var attachmentCounts = await dbContext.TicketAttachments.AsNoTracking()
            .Where(a => ids.Contains(a.ObjectId))
            .GroupBy(a => a.ObjectId)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, token);

        foreach (var item in items)
        {
            item.CommentCount = commentCounts.GetValueOrDefault(item.Id);
            item.AttachmentCount = attachmentCounts.GetValueOrDefault(item.Id);
            item.HasAttachment = item.AttachmentCount > 0;
            if (!staff && item.Comments != null)
                item.Comments = item.Comments.Where(c => !c.IsInternal).ToList();
        }
    }
}
