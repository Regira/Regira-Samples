using System.ComponentModel.DataAnnotations;
using HelpDesk.Api.Entities.Categories;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Priorities;
using HelpDesk.Api.Entities.Statuses;
using HelpDesk.Api.Entities.SupportTeams;
using Regira.Entities.Mapping.Models;
using Regira.Entities.Models;

namespace HelpDesk.Api.Entities.Tickets;

public record TicketSearchObject : SearchObject
{
    public ICollection<int>? StatusId { get; set; }
    public ICollection<int>? PriorityId { get; set; }
    public ICollection<int>? CategoryId { get; set; }
    public ICollection<int>? SupportTeamId { get; set; }
    public ICollection<int>? AssignedEmployeeId { get; set; }
    public ICollection<int>? CustomerId { get; set; }
    /// <summary>true = in a closed status; false = open queue.</summary>
    public bool? IsClosed { get; set; }
    public bool? IsAssigned { get; set; }
    /// <summary>Assigned to the signed-in employee.</summary>
    public bool? AssignedToMe { get; set; }
    /// <summary>Open and past its DueDate.</summary>
    public bool? IsOverdue { get; set; }
    public bool? HasAttachment { get; set; }
}

public enum TicketSortBy
{
    Default = 0,
    Newest,
    Oldest,
    Priority,
    DueDate,
    LastActivity,
    Code
}

[Flags]
public enum TicketIncludes
{
    Default = 0,
    Categories = 1 << 0,
    Comments = 1 << 1,
    Attachments = 1 << 2,
    All = Categories | Comments | Attachments
}

public class TicketDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int CustomerId { get; set; }
    public PersonCoreDto? Customer { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public PersonCoreDto? AssignedEmployee { get; set; }
    public int? SupportTeamId { get; set; }
    public SupportTeamCoreDto? SupportTeam { get; set; }
    public int PriorityId { get; set; }
    public PriorityDto? Priority { get; set; }
    public int StatusId { get; set; }
    public StatusDto? Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime? FirstResponseAt { get; set; }
    public ICollection<TicketCategoryDto>? Categories { get; set; }
    public ICollection<TicketCommentDto>? Comments { get; set; }
    public bool? HasAttachment { get; set; }
    public ICollection<EntityAttachmentDto>? Attachments { get; set; }
    public int? CommentCount { get; set; }
    public int? AttachmentCount { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class TicketCategoryDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CategoryId { get; set; }
    public CategoryCoreDto? Category { get; set; }
}

public class TicketCommentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AuthorId { get; set; }
    public PersonCoreDto? Author { get; set; }
    public string? Body { get; set; }
    public bool IsInternal { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class TicketInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(200)] public string? Title { get; set; }
    [MaxLength(8000)] public string? Description { get; set; }
    public int CustomerId { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public int? SupportTeamId { get; set; }
    public int PriorityId { get; set; }
    public int StatusId { get; set; }
    public DateTime? DueDate { get; set; }
    // nullable + uninitialized: an omitted collection maps as null = untouched (a status-only PATCH keeps them)
    public ICollection<TicketCategoryInputDto>? Categories { get; set; }
    public ICollection<EntityAttachmentInputDto>? Attachments { get; set; }
    // Comments deliberately absent: the comment action is their only writer (one writer per save path)
}

public class TicketCategoryInputDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int CategoryId { get; set; }
}

public class TicketCommentInputDto
{
    [Required, MaxLength(8000)] public string? Body { get; set; }
    public bool IsInternal { get; set; }
}
