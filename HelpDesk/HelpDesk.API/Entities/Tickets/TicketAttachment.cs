using Regira.Entities.Attachments.Models;

namespace HelpDesk.API.Entities.Tickets;

/// <summary>Per-owner attachment join (one extra simple slot) - the shared Attachment base is free infra</summary>
public class TicketAttachment : EntityAttachment
{
    public TicketAttachment() => ObjectType = nameof(Ticket);
}
