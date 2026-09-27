using EventPlanner.Api.Data;
using EventPlanner.Api.Entities.Categories;
using EventPlanner.Api.Entities.Events;
using EventPlanner.Api.Entities.Locations;
using EventPlanner.Api.Entities.Registrations;
using EventPlanner.Api.Entities.Speakers;
using EventPlanner.Api.Entities.Users;
using EventPlanner.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Web.DependencyInjection;
using Regira.Security.Authentication.Core.Models;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Regira.Web.Routing;
using Scalar.AspNetCore;

namespace EventPlanner.Api.Infrastructure;

public static class HostingExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers(o =>
        {
            o.UseCentralRoutePrefix(new RouteAttribute("api"));
            o.Filters.Add<WriteAuthorizationFilter>();
        });
        services.ConfigureDefaultJsonOptions();
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<AuthenticationSchemeDocumentTransformer>();
            options.AddOperationTransformer<SecurityRequirementOperationTransformer>();
        });
        services.AddHttpContextAccessor();
        return services;
    }

    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
        => services.AddDbContext<EventPlannerDbContext>(options => options.UseSqlite(configuration.GetConnectionString("Default")));

    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AppUser>(o =>
            {
                // JWT validates roles against "role" — Identity's default is the long ClaimTypes.Role URI
                o.ClaimsIdentity.RoleClaimType = RegiraClaimTypes.Role;
                o.User.RequireUniqueEmail = true;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<EventPlannerDbContext>()
            .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.AddTransient<IEmailSender, LoggingEmailSender>();

        services.AddJwtAuthentication(o => configuration.GetSection(AuthenticationSections.Jwt).Bind(o))
            .AddRefreshTokens(); // in-memory store (development)

        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());
        return services;
    }

    // Free-tier budget (5 simple + 2 complex):
    // | Entity              | Classification                         | Tally        |
    // | Location            | simple (int, LocationSearchObject)     | 1/5 simple   |
    // | Speaker             | simple (int, SpeakerSearchObject)      | 2/5 simple   |
    // | EventCategory       | simple                                 | 3/5 simple   |
    // | Event               | complex (EventSortBy/EventIncludes)    | 1/2 complex  |
    // | Session             | owned by Event via Related() — no slot | -            |
    // | SessionSpeaker      | owned by Session (nested Related())    | -            |
    // | Registration        | complex                                | 2/2 complex  |
    // | RegistrationSession | owned by Registration via Related()    | -            |
    // Employees are ASP.NET Identity users (AppUser), not Regira entities.
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services
            .UseEntities<EventPlannerDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.AddGlobalFilterQueryBuilder<EventVisibilityQueryBuilder>();
                options.AddGlobalFilterQueryBuilder<RegistrationScopeQueryBuilder>();
            })
            .AddLocations()
            .AddSpeakers()
            .AddEventCategories()
            .AddEvents()
            .AddRegistrations();
        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.Authentication = new ScalarAuthenticationOptions { PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme] };
        }).AllowAnonymous();

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        return app;
    }
}
