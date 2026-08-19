using EventPlanner.Api;
using EventPlanner.Api.Data;
using EventPlanner.Api.Extensions;
using EventPlanner.Api.Seeding;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
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

    // Central "api" route prefix — keep controller routes resource-relative, prefix applied once here.
    builder.Services.AddControllers(o => o.Conventions.Add(new RoutePrefixConvention("api")));
    // Applies ReferenceHandler.IgnoreCycles + JsonStringEnumConverter + WhenWritingNull to both the
    // MVC JsonOptions and Http.Json.JsonOptions — required so AddOpenApi()'s schema matches the wire format.
    builder.Services.ConfigureDefaultJsonOptions();

    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<EventPlannerDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddEntityServices();

    // Dev SPA on a different origin (Vite dev server) needs CORS.
    builder.Services.AddCors(o => o.AddPolicy("SpaDev", policy => policy
        .SetIsOriginAllowed(origin => Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("SpaDev");

    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EventPlannerDbContext>();
        dbContext.Database.EnsureCreated();
        await DataSeeder.SeedAsync(scope.ServiceProvider);
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

namespace EventPlanner.Api
{
    // Applies a shared "api" prefix once at the host level so controller routes stay resource-relative.
    public sealed class RoutePrefixConvention(string prefix) : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _prefix = new(new Microsoft.AspNetCore.Mvc.RouteAttribute(prefix));
        public void Apply(ApplicationModel app)
        {
            foreach (var controller in app.Controllers)
                foreach (var selector in controller.Selectors)
                    selector.AttributeRouteModel = selector.AttributeRouteModel is { } existing
                        ? AttributeRouteModel.CombineAttributeRouteModel(_prefix, existing)
                        : _prefix;
        }
    }
}
