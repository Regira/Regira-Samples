using System.ComponentModel.DataAnnotations;
using Regira.Entities.Mapping.Models;

namespace HelpDesk.API.Entities.Tickets;

public class TicketInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    [MaxLength(8000)] public string? Description { get; set; }

    [Required] public int CustomerId { get; set; }
    public int? AssignedEmployeeId { get; set; }
    [Required] public int PriorityId { get; set; }
    [Required] public int StatusId { get; set; }
    public int? SupportTeamId { get; set; }

    // ClosedAt is server-owned - stamped/cleared by a prepper from the target Status.IsClosed, see
    // TicketServiceConfiguration. Deliberately absent here so a client can never set/tamper with it.

    // Configured with e.Related() on Ticket - null = untouched, [] = clears all, populated = replace set
    public ICollection<TicketCategoryInputDto>? Categories { get; set; }

    // Declared so the attachment sync doesn't warn at startup; normal uploads go through the
    // dedicated {id}/files sub-routes and leave this null (untouched).
    public ICollection<EntityAttachmentInputDto>? Attachments { get; set; }
}
