using HelpDesk.Api.Data;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Entities.Tickets;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Services.Abstractions;

namespace HelpDesk.Api.Controllers;

/// <summary>
/// The conversation thread. Comments are an owned child of the ticket with one writer: this action (the ticket
/// input DTO never carries them). The author is always the caller; customers cannot post internal notes.
/// </summary>
[ApiController, Route("tickets")]
public class TicketCommentsController(IEntityService<Ticket, int> tickets, HelpDeskDbContext dbContext, CurrentUser currentUser) : ControllerBase
{
    [HttpPost("{id:int}/comments")]
    public async Task<IActionResult> Add(int id, [FromBody] TicketCommentInputDto input, CancellationToken token)
    {
        // read through the entity service: the row filter answers 404 for a ticket the caller cannot see
        var ticket = await tickets.Details(id, token);
        if (ticket == null) return NotFound();

        var authorId = await currentUser.GetPersonId(token);
        if (authorId == null)
        {
            ModelState.AddModelError("AuthorId", "Your account is not linked to a person profile.");
            return BadRequest(ModelState);
        }
        if (input.IsInternal && !currentUser.IsStaff)
        {
            ModelState.AddModelError(nameof(input.IsInternal), "Only staff can add internal notes.");
            return BadRequest(ModelState);
        }

        var now = DateTime.UtcNow;
        var comment = new TicketComment
        {
            TicketId = id,
            AuthorId = authorId.Value,
            Body = input.Body!.Trim(),
            IsInternal = input.IsInternal,
            Created = now
        };
        dbContext.TicketComments.Add(comment);

        // touch the ticket (LastActivity sort) and stamp the first public staff response
        var stored = await dbContext.Tickets.FirstAsync(t => t.Id == id, token);
        stored.LastModified = now;
        if (currentUser.IsStaff && !input.IsInternal && stored.FirstResponseAt == null)
            stored.FirstResponseAt = now;
        await dbContext.SaveChangesAsync(token);

        var dto = await dbContext.TicketComments.AsNoTracking()
            .Where(c => c.Id == comment.Id)
            .Select(c => new TicketCommentDto
            {
                Id = c.Id,
                TicketId = c.TicketId,
                AuthorId = c.AuthorId,
                Author = new PersonCoreDto
                {
                    Id = c.Author!.Id,
                    Role = c.Author.Role,
                    GivenName = c.Author.GivenName,
                    FamilyName = c.Author.FamilyName,
                    FullName = c.Author.GivenName + " " + c.Author.FamilyName,
                    Email = c.Author.Email,
                    Company = c.Author.Company,
                    JobTitle = c.Author.JobTitle
                },
                Body = c.Body,
                IsInternal = c.IsInternal,
                Created = c.Created,
                LastModified = c.LastModified
            })
            .FirstAsync(token);
        return Ok(new { item = dto });
    }
}
