using Regira.Entities.Mapping.Models;
using HelpDesk.API.Entities.People;
using HelpDesk.API.Entities.Priorities;
using HelpDesk.API.Entities.Statuses;
using HelpDesk.API.Entities.SupportTeams;

namespace HelpDesk.API.Entities.Tickets;

public class TicketDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public int CustomerId { get; set; }
    public PersonCoreDto? Customer { get; set; }

    public int? AssignedEmployeeId { get; set; }
    public PersonCoreDto? AssignedEmployee { get; set; }

    public int PriorityId { get; set; }
    public PriorityCoreDto? Priority { get; set; }

    public int StatusId { get; set; }
    public StatusCoreDto? Status { get; set; }

    public int? SupportTeamId { get; set; }
    public SupportTeamCoreDto? SupportTeam { get; set; }

    public DateTime? ClosedAt { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }

    public ICollection<TicketCategoryDto>? Categories { get; set; }
    public ICollection<TicketCommentDto>? Comments { get; set; }

    public bool? HasAttachment { get; set; }
    public ICollection<EntityAttachmentDto>? Attachments { get; set; }
}
