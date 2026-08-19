using AssetHub.Api.Data;
using AssetHub.Api.Extensions;
using AssetHub.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
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

    builder.Services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
    builder.Services.ConfigureDefaultJsonOptions();

    builder.Services.AddDbContext<AppDbContext>(options => options
        .UseSqlite(builder.Configuration.GetConnectionString("Default"))
        // Asset is the aggregate parent of Attachments/Warranties/MaintenanceRecords -- the query-filter/
        // required-navigation warning EF logs for those relationships is the expected soft-delete shape.
        .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

    builder.Services.AddEntityServices();

    builder.Services.AddOpenApi();

    // Dev SPA on a different origin (localhost:6101) needs CORS.
    builder.Services.AddCors(o => o.AddPolicy("Spa", p => p
        .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()));

    var app = builder.Build();

    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference();

    app.UseCors("Spa");

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
        await SeedData.SeedAsync(scope.ServiceProvider);
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
