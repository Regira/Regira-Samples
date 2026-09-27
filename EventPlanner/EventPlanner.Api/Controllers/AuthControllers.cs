using EventPlanner.Api.Entities.Users;
using EventPlanner.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Security.Authentication.Web.Models;

namespace EventPlanner.Api.Controllers;

public class AccountController(ITokenHelper tokenHelper, UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> claimsFactory, ILogger<AccountController> logger)
    : AccountControllerBase<AppUser>(tokenHelper, userManager, claimsFactory, logger);

public class PasswordController(UserManager<AppUser> userManager)
    : PasswordControllerBase<AppUser>(userManager);

public class UsersController(UserManager<AppUser> userManager)
    : UserControllerBase<AppUser>(userManager)
{
    // creating accounts is an administrator task (the base only requires a signed-in caller)
    [Authorize(Roles = Roles.Admin)]
    public override Task<IActionResult> Create(UserInput model, [FromServices] IEmailSender mailer) => base.Create(model, mailer);
}
