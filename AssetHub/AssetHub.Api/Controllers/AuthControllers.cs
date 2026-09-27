using AssetHub.Api.Data;
using AssetHub.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;
using Regira.Security.Authentication.Web.Models;

namespace AssetHub.Api.Controllers;

// Route templates + [ApiController] are inherited from the bases (auth, auth/password, users)
public class AccountController(ITokenHelper tokenHelper, UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> claimsFactory, ILogger<AccountController> logger)
    : AccountControllerBase<AppUser>(tokenHelper, userManager, claimsFactory, logger);

public class PasswordController(UserManager<AppUser> userManager)
    : PasswordControllerBase<AppUser>(userManager);

public class UsersController(UserManager<AppUser> userManager)
    : UserControllerBase<AppUser>(userManager)
{
    // POST users carries no role check of its own: only administrators may create accounts
    [Authorize(Roles = Roles.Admin)]
    public override Task<IActionResult> Create(UserInput model, [FromServices] IEmailSender mailer) => base.Create(model, mailer);
}
