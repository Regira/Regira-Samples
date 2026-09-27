using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using Scalar.AspNetCore;
using Serilog;
using ShopMate.Api.Data;
using ShopMate.Api.Data.Seeding;
using ShopMate.Api.Extensions;

namespace ShopMate.Api.Infrastructure;

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

        // every controller lives under /api (the SPA's axios base + Vite proxy agree on it)
        builder.Services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
        builder.Services.ConfigureDefaultJsonOptions();
        builder.Services.AddOpenApi();

        var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        builder.Services.AddCors(o => o.AddPolicy(SpaCorsPolicy, p => p
            .SetIsOriginAllowed(origin => origins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            .AllowAnyHeader()
            .AllowAnyMethod()));

        builder.Services.AddDbContext<ShopMateDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddEntityServices();
        builder.Services.AddScoped<DataSeeder>();

        return builder;
    }

    public static async Task<WebApplication> InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopMateDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await dbContext.Database.EnsureDeletedAsync();   // dotnet run -- --ResetDatabase=true
        await dbContext.Database.EnsureCreatedAsync();

        if (app.Configuration.GetValue("Seeding:Enabled", true))
            await scope.ServiceProvider.GetRequiredService<DataSeeder>().Seed();
        return app;
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        app.MapOpenApi();
        app.MapScalarApiReference();

        // the dev SPA talks plain HTTP (through the Vite proxy) -> no redirect in Development
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors(SpaCorsPolicy);
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
