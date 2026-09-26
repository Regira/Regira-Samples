using GeniusTest.Api.Data;
using GeniusTest.Api.Data.Seeding;
using GeniusTest.Api.Services.Content;
using GeniusTest.Api.Services.Play;
using GeniusTest.Api.Services.Spin;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;

namespace GeniusTest.Api.Infrastructure;

public static class HostingExtensions
{
    public static WebApplicationBuilder AddGeniusServices(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes = true;
        });

        // no "api" route prefix: IIS mounts the API as the /stablegenius/api application, the Vite dev proxy strips /api
        builder.Services.AddControllers();
        builder.Services.ConfigureDefaultJsonOptions();
        builder.Services.AddOpenApi();

        builder.Services.AddDbContext<GeniusDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
        builder.Services.AddEntityServices();

        builder.Services.AddSingleton(Random.Shared);
        // title ladder, leaderboard legends and the API's own screen texts (titles/legends/game-texts.csv), read once at startup
        var contentRoot = builder.Environment.ContentRootPath;
        builder.Services.AddSingleton(_ => new GameContentStore(GameContent.Load(contentRoot).GetAwaiter().GetResult()));
        // per request: the content as it was when the request started (a reset swaps the store, not this)
        builder.Services.AddScoped(sp => sp.GetRequiredService<GameContentStore>().Current);
        builder.Services.AddScoped<SpinDoctor>(sp => new SpinDoctor(sp.GetRequiredService<Random>(), sp.GetRequiredService<GameContent>()));
        builder.Services.AddScoped<PlayService>();
        builder.Services.AddScoped<GeniusSeeder>();
        builder.Services.AddScoped<ResetGate>();
        return builder;
    }

    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GeniusDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await db.Database.EnsureDeletedAsync();   // dotnet run -- --ResetDatabase=true
        await db.Database.EnsureCreatedAsync();
        await scope.ServiceProvider.GetRequiredService<GeniusSeeder>().Seed();
    }

    public static WebApplication UseGeniusPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
