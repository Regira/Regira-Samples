using Microsoft.AspNetCore.Identity;
using QCredits.Api.Data;
using Regira.Security.Authentication.Jwt.Abstraction;
using Regira.Security.Authentication.Web.Controllers;

namespace QCredits.Api.Controllers;

// Routes and [ApiController] are inherited from the Regira base controllers.
public class AccountController(ITokenHelper tokenHelper, UserManager<AppUser> userManager,
    IUserClaimsPrincipalFactory<AppUser> claimsFactory, ILogger<AccountController> logger)
    : AccountControllerBase<AppUser>(tokenHelper, userManager, claimsFactory, logger);

public class PasswordController(UserManager<AppUser> userManager)
    : PasswordControllerBase<AppUser>(userManager);
