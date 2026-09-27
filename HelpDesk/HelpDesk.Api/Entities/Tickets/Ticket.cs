using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using Regira.Entities.Attachments.Abstractions;
using Regira.Entities.Attachments.Models;
using Regira.Entities.Attributes;
using Regira.Entities.Models.Abstractions;

namespace HelpDesk.Api.Entities.Tickets;

public class Ticket : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasDescription, IHasCode, IHasNormalizedContent,
    IHasAttachments, IHasAttachments<TicketAttachment>
{
    public int Id { get; set; }
    /// <summary>Sequential reference (HD-000123), minted on create by the prepper.</summary>
    [ServerOwned, MaxLength(16)] public string? Code { get; set; }
    /// <summary>The subject line.</summary>
    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(8000)] public string? Description { get; set; }

    public int CustomerId { get; set; }
    public Person? Customer { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public Person? AssignedEmployee { get; set; }
    public int? SupportTeamId { get; set; }
    public SupportTeam? SupportTeam { get; set; }
    public int PriorityId { get; set; }
    public Priority? Priority { get; set; }
    public int StatusId { get; set; }
    public Status? Status { get; set; }

    /// <summary>SLA target; defaults to Created + Priority.TargetHours.</summary>
    public DateTime? DueDate { get; set; }
    /// <summary>Stamped when the ticket enters a closed status, cleared when reopened (prepper-owned).</summary>
    public DateTime? ClosedAt { get; set; }
    /// <summary>First public staff reply (written by the comment action only).</summary>
    [ServerOwned] public DateTime? FirstResponseAt { get; set; }

    public ICollection<TicketCategory>? Categories { get; set; }
    /// <summary>Conversation; its only writer is the comment action (TicketCommentsController).</summary>
    public ICollection<TicketComment>? Comments { get; set; }

    [NotMapped] public bool? HasAttachment { get; set; }
    public ICollection<TicketAttachment>? Attachments { get; set; }
    ICollection<IEntityAttachment>? IHasAttachments.Attachments
    {
        get => Attachments?.Cast<IEntityAttachment>().ToArray();
        set => Attachments = value?.Cast<TicketAttachment>().ToArray();
    }

    /// <summary>Filled by TicketProcessor (visible comments only).</summary>
    [NotMapped] public int? CommentCount { get; set; }
    [NotMapped] public int? AttachmentCount { get; set; }

    [MaxLength(1024)] public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

/// <summary>Many-to-many join, owned by the ticket (synced via e.Related).</summary>
public class TicketCategory : IEntityWithSerial
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

public class TicketComment : IEntityWithSerial, IHasTimestamps
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Ticket? Ticket { get; set; }
    public int AuthorId { get; set; }
    public Person? Author { get; set; }
    [Required, MaxLength(8000)] public string? Body { get; set; }
    /// <summary>Internal notes are visible to staff only.</summary>
    public bool IsInternal { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class TicketAttachment : EntityAttachment
{
    public TicketAttachment() => ObjectType = nameof(Ticket);
}
