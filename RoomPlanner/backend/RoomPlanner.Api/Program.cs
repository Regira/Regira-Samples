using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using RoomPlanner.Api.Data;
using RoomPlanner.Api.Extensions;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateOnBuild = true;
        o.ValidateScopes = true;
    });

    builder.Services.AddControllers(o => o.Conventions.Add(new RoutePrefixConvention("api")));
    builder.Services.ConfigureDefaultJsonOptions(); // cycles, nulls, enum-as-names - controllers AND Http.Json.JsonOptions
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<RoomPlannerDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    // adds entity services (repositories, mapping) for every registered entity - see Extensions/ServiceCollectionExtensions
    builder.Services.AddEntityServices();

    builder.Services.AddCors(o => o.AddPolicy("Spa", policy => policy
        .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    // no HTTPS configured for this demo - skip the redirect so the dev SPA can call over plain HTTP
    app.UseCors("Spa");

    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<RoomPlannerDbContext>();
        dbContext.Database.EnsureCreated();
        await RoomPlanner.Api.Data.Seeding.SeedDataGenerator.SeedAsync(scope.ServiceProvider);
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>Applies a route prefix (e.g. "api") to every controller. Kept simple/self-contained for this demo.</summary>
public sealed class RoutePrefixConvention(string prefix) : Microsoft.AspNetCore.Mvc.ApplicationModels.IApplicationModelConvention
{
    private readonly Microsoft.AspNetCore.Mvc.ApplicationModels.AttributeRouteModel _prefix = new(new Microsoft.AspNetCore.Mvc.RouteAttribute(prefix));

    public void Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ApplicationModel app)
    {
        foreach (var controller in app.Controllers)
        {
            foreach (var selector in controller.Selectors)
            {
                selector.AttributeRouteModel = selector.AttributeRouteModel is { } existing
                    ? Microsoft.AspNetCore.Mvc.ApplicationModels.AttributeRouteModel.CombineAttributeRouteModel(_prefix, existing)
                    : _prefix;
            }
        }
    }
}
