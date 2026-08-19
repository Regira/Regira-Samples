using System.ComponentModel.DataAnnotations;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;
using HelpDesk.API.Entities.Categories;
using HelpDesk.API.Entities.People;
using HelpDesk.API.Entities.Priorities;
using HelpDesk.API.Entities.Statuses;
using HelpDesk.API.Entities.SupportTeams;

namespace HelpDesk.API.Entities.Tickets;

public class Ticket : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasNormalizedContent,
    IHasAttachments, IHasAttachments<TicketAttachment>
{
    public int Id { get; set; }

    /// <summary>Ticket subject</summary>
    [Required, MaxLength(160)] public string Title { get; set; } = null!;
    [MaxLength(8000)] public string? Description { get; set; }

    public int CustomerId { get; set; }
    public Person? Customer { get; set; }

    public int? AssignedEmployeeId { get; set; }
    public Person? AssignedEmployee { get; set; }

    public int PriorityId { get; set; }
    public Priority? Priority { get; set; }

    public int StatusId { get; set; }
    public Status? Status { get; set; }

    public int? SupportTeamId { get; set; }
    public SupportTeam? SupportTeam { get; set; }

    public DateTime? ClosedAt { get; set; }

    [MaxLength(2048), Normalized(SourceProperties = [nameof(Title), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    // m2m join, owned via e.Related()
    public ICollection<TicketCategory>? Categories { get; set; }

    // conversation thread - read-only navigation, managed through custom TicketController actions
    public ICollection<TicketComment>? Comments { get; set; }

    // attachments
    public bool? HasAttachment { get; set; }
    public ICollection<TicketAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<TicketAttachment>().ToArray();
    }
}
