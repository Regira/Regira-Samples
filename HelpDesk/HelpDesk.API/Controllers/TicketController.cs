using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.Controllers.Abstractions;
using HelpDesk.API.Data;
using HelpDesk.API.Entities.Tickets;

namespace HelpDesk.API.Controllers;

[ApiController, Route("tickets")]
public class TicketController(AppDbContext dbContext)
    : EntityControllerBase<Ticket, TicketSearchObject, TicketSortBy, TicketIncludes, TicketDto, TicketInputDto>
{
    // Conversation thread: comments are NOT synced through the Ticket save path (see TicketComment.cs) -
    // posting one comment never requires resending the whole thread.

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult> GetComments(int id)
    {
        var exists = await dbContext.Tickets.AnyAsync(t => t.Id == id);
        if (!exists) return NotFound();

        var comments = await dbContext.TicketComments
            .Where(c => c.TicketId == id)
            .OrderBy(c => c.Created)
            .Include(c => c.Author)
            .Select(c => new TicketCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                AuthorId = c.AuthorId,
                Author = c.Author == null ? null : new Entities.People.PersonCoreDto
                {
                    Id = c.Author.Id,
                    FullName = c.Author.FullName,
                    Email = c.Author.Email,
                    Role = c.Author.Role,
                    IsActive = c.Author.IsActive
                },
                Message = c.Message,
                IsInternal = c.IsInternal,
                Created = c.Created
            })
            .ToListAsync();

        return Ok(new { items = comments });
    }

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult> AddComment(int id, [FromBody] TicketCommentInputDto input)
    {
        var ticket = await dbContext.Tickets.FirstOrDefaultAsync(t => t.Id == id);
        if (ticket == null) return NotFound();

        var author = await dbContext.People.FirstOrDefaultAsync(p => p.Id == input.AuthorId);
        if (author == null) return BadRequest(new { error = "Unknown author" });

        var comment = new TicketComment
        {
            TicketId = id,
            AuthorId = input.AuthorId,
            Message = input.Message,
            IsInternal = input.IsInternal,
            Created = DateTime.UtcNow
        };
        dbContext.TicketComments.Add(comment);
        ticket.LastModified = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();

        return Ok(new
        {
            item = new TicketCommentDto
            {
                Id = comment.Id,
                TicketId = comment.TicketId,
                AuthorId = comment.AuthorId,
                Author = new Entities.People.PersonCoreDto
                {
                    Id = author.Id,
                    FullName = author.FullName,
                    Email = author.Email,
                    Role = author.Role,
                    IsActive = author.IsActive
                },
                Message = comment.Message,
                IsInternal = comment.IsInternal,
                Created = comment.Created
            }
        });
    }
}
