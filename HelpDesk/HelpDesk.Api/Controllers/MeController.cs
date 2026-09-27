using HelpDesk.Api.Data;
using HelpDesk.Api.Entities.Persons;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Api.Controllers;

/// <summary>The signed-in caller: roles and the linked person profile (author of comments, owner of tickets).</summary>
[ApiController, Route("me")]
public class MeController(HelpDeskDbContext dbContext, CurrentUser currentUser) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken token)
    {
        var userId = currentUser.UserId;
        var person = userId == null
            ? null
            : await dbContext.Persons.AsNoTracking().Include(p => p.SupportTeam).FirstOrDefaultAsync(p => p.UserId == userId, token);
        return Ok(new
        {
            item = new
            {
                userId,
                roles = currentUser.Roles,
                isStaff = currentUser.IsStaff,
                isAdmin = currentUser.IsAdmin,
                person = person == null ? null : new PersonDto
                {
                    Id = person.Id,
                    Role = person.Role,
                    GivenName = person.GivenName,
                    FamilyName = person.FamilyName,
                    FullName = person.FullName,
                    Email = person.Email,
                    Phone = person.Phone,
                    Company = person.Company,
                    JobTitle = person.JobTitle,
                    IsActive = person.IsActive,
                    SupportTeamId = person.SupportTeamId,
                    HasAccount = true,
                    Created = person.Created,
                    LastModified = person.LastModified
                }
            }
        });
    }
}

public class AccountInput
{
    public string? Password { get; set; }
}

/// <summary>Admin action: give a person a login (role derived from Person.Role) and link it.</summary>
[ApiController, Route("persons")]
public class PersonAccountsController(HelpDeskDbContext dbContext, UserManager<AppUser> userManager) : ControllerBase
{
    [HttpPost("{id:int}/account")]
    public async Task<IActionResult> Create(int id, [FromBody] AccountInput input, CancellationToken token)
    {
        var person = await dbContext.Persons.FirstOrDefaultAsync(p => p.Id == id, token);
        if (person == null) return NotFound();
        if (person.UserId != null) { ModelState.AddModelError("UserId", "This person already has an account."); return BadRequest(ModelState); }
        if (string.IsNullOrWhiteSpace(person.Email)) { ModelState.AddModelError("Email", "An e-mail address is required to create an account."); return BadRequest(ModelState); }

        var user = new AppUser { UserName = person.Email, Email = person.Email, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, input.Password ?? "");
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("Password", error.Description);
            return BadRequest(ModelState);
        }
        await userManager.AddToRoleAsync(user, person.Role == PersonRole.Employee ? Roles.Agent : Roles.Customer);
        person.UserId = user.Id;
        await dbContext.SaveChangesAsync(token);
        return Ok(new { item = new { person.Id, hasAccount = true, userName = user.UserName } });
    }
}
