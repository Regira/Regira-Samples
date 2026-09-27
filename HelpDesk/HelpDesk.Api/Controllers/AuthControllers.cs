using HelpDesk.Api.Data;
using Microsoft.AspNetCore.Identity;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;

namespace HelpDesk.Api.Controllers;

// routes (auth, auth/password, users) and [ApiController] are inherited from the bases
public class AccountController(ITokenHelper tokenHelper, UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> claimsFactory, ILogger<AccountController> logger)
    : AccountControllerBase<AppUser>(tokenHelper, userManager, claimsFactory, logger);

public class PasswordController(UserManager<AppUser> userManager)
    : PasswordControllerBase<AppUser>(userManager);

// POST users is gated to Admin by the WriteAuthorizationFilter ("Users")
public class UsersController(UserManager<AppUser> userManager)
    : UserControllerBase<AppUser>(userManager);
