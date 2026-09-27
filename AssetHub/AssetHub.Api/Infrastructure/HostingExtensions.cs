using AssetHub.Api.Data;
using AssetHub.Api.Data.Seeding;
using AssetHub.Api.Extensions;
using AssetHub.Api.Infrastructure.Security;
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

namespace AssetHub.Api.Infrastructure;

public static class HostingExtensions
{
    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
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

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddAppAuthentication(configuration);
        services.AddEntityServices(configuration);

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<AuthenticationSchemeDocumentTransformer>();
            options.AddOperationTransformer<SecurityRequirementOperationTransformer>();
        });

        return builder;
    }

    private static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AppUser>(o =>
            {
                // align Identity's role claim with the JWT scheme's "role" claim
                o.ClaimsIdentity.RoleClaimType = RegiraClaimTypes.Role;
                o.User.RequireUniqueEmail = true;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddTransient<IEmailSender, DevEmailSender>();

        services
            .AddJwtAuthentication(o => configuration.GetSection(AuthenticationSections.Jwt).Bind(o))
            .AddRefreshTokens();

        return services;
    }

    public static async Task<WebApplication> ConfigurePipeline(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
                await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();
            await DataSeeder.Seed(scope.ServiceProvider, app.Configuration);
        }

        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference(options =>
        {
            options.Authentication = new ScalarAuthenticationOptions
            {
                PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme]
            };
        }).AllowAnonymous();

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        else
            // the Vite dev proxy forwards the SPA's host (xfwd) so attachment URIs stay on the SPA origin
            app.UseForwardedHeaders(new ForwardedHeadersOptions { ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto });

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers().RequireAuthorization();

        return app;
    }
}
