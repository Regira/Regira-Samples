using Fleet.Api.Data;
using Fleet.Api.Data.Seeding;
using Fleet.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using Scalar.AspNetCore;

namespace Fleet.Api.Infrastructure;

public static class HostingExtensions
{
    public static WebApplicationBuilder AddFleetServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;

        // every controller lives under /api (the SPA's axios base + Vite proxy use the same prefix)
        services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
        // cycles/nulls/enum names/UTC reads on both MVC + Http.Json options, plus the 400/409 exception filter
        services.ConfigureDefaultJsonOptions();
        services.AddOpenApi();

        services.AddDbContext<FleetDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        services.AddEntityServices();
        services.AddScoped<FleetSeeder>();

        return builder;
    }

    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await db.Database.EnsureDeletedAsync();
        var created = await db.Database.EnsureCreatedAsync();
        if (created || !await db.Vehicles.AnyAsync())
            await scope.ServiceProvider.GetRequiredService<FleetSeeder>().Seed();
    }

    public static WebApplication UseFleetPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();

        // the dev SPA proxies plain HTTP; only redirect outside Development
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
