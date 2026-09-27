using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using Scalar.AspNetCore;
using Webshop.Api.Data;
using Webshop.Api.Data.Seeding;
using Webshop.Api.Extensions;

namespace Webshop.Api.Infrastructure;

public static class HostingExtensions
{
    public static IServiceCollection AddWebshopApi(this IServiceCollection services, IConfiguration configuration)
    {
        // every controller is served under /api (the SPA dev proxy forwards /api to this host)
        services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
        // cycles/nulls/enum names/UTC dates on MVC + Http.Json options, plus the entity exception filter (400/409)
        services.ConfigureDefaultJsonOptions();
        services.AddOpenApi();

        services.AddDbContext<WebshopDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddEntityServices();
        services.AddScoped<WebshopSeeder>();

        return services;
    }

    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();

        if (app.Configuration.GetValue("Seeding:Enabled", true))
            await scope.ServiceProvider.GetRequiredService<WebshopSeeder>().Seed();
    }

    public static WebApplication UseWebshopPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();

        // the dev SPA talks to this API over plain HTTP through the Vite proxy
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
