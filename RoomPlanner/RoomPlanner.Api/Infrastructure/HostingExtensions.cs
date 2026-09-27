using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using RoomPlanner.Api.Data;
using RoomPlanner.Api.Data.Seeding;
using RoomPlanner.Api.Extensions;
using Scalar.AspNetCore;
using Serilog;

namespace RoomPlanner.Api.Infrastructure;

public static class HostingExtensions
{
    public const string SpaCorsPolicy = "spa";

    public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes = true;
        });

        // every controller lives under /api (the SPA's axios base + Vite proxy use the same prefix)
        builder.Services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
        builder.Services.ConfigureDefaultJsonOptions();
        builder.Services.AddOpenApi();

        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        builder.Services.AddCors(o => o.AddPolicy(SpaCorsPolicy, p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        builder.Services.AddEntityServices();
        builder.Services.AddScoped<DataSeeder>();
        return builder;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();

        // the dev SPA proxies over plain http -> no https redirect in Development
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors(SpaCorsPolicy);
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await db.Database.EnsureDeletedAsync();   // dotnet run -- --ResetDatabase=true
        var created = await db.Database.EnsureCreatedAsync();

        if (created && app.Configuration.GetValue("Seeding:Enabled", true))
        {
            var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
            await seeder.Seed(app.Configuration.GetValue("Seeding:Reservations", 500));
        }
    }
}
