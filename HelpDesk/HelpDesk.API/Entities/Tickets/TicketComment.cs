using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using HelpDesk.API.Entities.People;

namespace HelpDesk.API.Entities.Tickets;

/// <summary>
/// A single message in a ticket's conversation thread. Managed through dedicated actions on
/// TicketController (custom routes, raw DbContext) rather than e.Related() or its own .For<>() slot -
/// so posting one comment never requires resending the whole thread (see entities.patterns "Domain
/// actions on an entity resource").
/// </summary>
public class TicketComment : IEntityWithSerial, IHasCreated
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int AuthorId { get; set; }
    public Person? Author { get; set; }
    [Required, MaxLength(4000)] public string Message { get; set; } = null!;

    /// <summary>Internal notes are visible to agents/admins only, not to the customer</summary>
    public bool IsInternal { get; set; }
    public DateTime Created { get; set; }
}
