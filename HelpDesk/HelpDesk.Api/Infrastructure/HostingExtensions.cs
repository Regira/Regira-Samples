using HelpDesk.Api.Data;
using HelpDesk.Api.Extensions;
using HelpDesk.Api.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Security.Authentication.Core.Models;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Regira.Web.Routing;
using Scalar.AspNetCore;

namespace HelpDesk.Api.Infrastructure;

public static class HostingExtensions
{
    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes = true;
        });

        services.AddControllers(o =>
        {
            o.UseCentralRoutePrefix(new RouteAttribute("api"));
            o.Filters.Add<WriteAuthorizationFilter>();
        });
        services.ConfigureDefaultJsonOptions();

        services.AddDbContext<HelpDeskDbContext>(o => o.UseSqlite(configuration.GetConnectionString("Default")));
        services.AddEntityServices(configuration);

        // Identity (users in the same database) + roles carried into the JWT as "role"
        services.AddIdentityCore<AppUser>(o =>
            {
                o.ClaimsIdentity.RoleClaimType = RegiraClaimTypes.Role;
                o.User.RequireUniqueEmail = true;
                o.Password.RequiredLength = 8;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<HelpDeskDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();
        services.AddTransient<IEmailSender, DevEmailSender>();

        services
            .AddJwtAuthentication(o => configuration.GetSection(AuthenticationSections.Jwt).Bind(o))
            .AddRefreshTokens();
        services.AddAuthorization();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<AuthenticationSchemeDocumentTransformer>();
            options.AddOperationTransformer<SecurityRequirementOperationTransformer>();
        });

        return builder;
    }

    public static WebApplication UsePipeline(this WebApplication app)
    {
        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme]
            };
        }).AllowAnonymous();

        // the dev SPA proxies plain HTTP; redirect only outside Development. The Vite proxy forwards the SPA's
        // origin (xfwd), so absolute URLs the API builds (attachment uri) point at the SPA, not at :5841.
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        else
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto });

        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();
        return app;
    }
}
