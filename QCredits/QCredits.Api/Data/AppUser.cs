using Microsoft.AspNetCore.Identity;

namespace QCredits.Api.Data;

/// <summary>Login account. Linked to an <c>Employee</c> by e-mail address.</summary>
public class AppUser : IdentityUser;
