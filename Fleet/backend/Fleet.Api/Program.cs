using Fleet.Api.Data;
using Fleet.Api.Extensions;
using Fleet.Api.Infrastructure;
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

    // Resource-relative controller routes ("vehicles", "interventions", ...) + one global "api" prefix.
    builder.Services.AddControllers(o => o.Conventions.Add(new RoutePrefixConvention("api")));
    builder.Services.ConfigureDefaultJsonOptions();
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<FleetDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddEntityServices();

    builder.Services.AddCors(o => o.AddPolicy("Spa", p => p
        .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
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

    app.UseCors("Spa");

    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        dbContext.Database.EnsureCreated();
        await FleetSeeder.SeedAsync(scope.ServiceProvider);
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
