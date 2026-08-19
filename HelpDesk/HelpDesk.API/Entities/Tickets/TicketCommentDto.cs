using System.ComponentModel.DataAnnotations;
using HelpDesk.API.Entities.People;

namespace HelpDesk.API.Entities.Tickets;

public class TicketCommentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public int AuthorId { get; set; }
    public PersonCoreDto? Author { get; set; }
    public string Message { get; set; } = null!;
    public bool IsInternal { get; set; }
    public DateTime Created { get; set; }
}

/// <summary>Body for POST /tickets/{id}/comments - a narrow, additive action (not a Related() sync)</summary>
public class TicketCommentInputDto
{
    [Required] public int AuthorId { get; set; }
    [Required, MaxLength(4000)] public string Message { get; set; } = null!;
    public bool IsInternal { get; set; }
}
