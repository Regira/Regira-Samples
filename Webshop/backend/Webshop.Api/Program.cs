using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;
using Webshop.Api.Data;
using Webshop.Api.Infrastructure;

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

    // Resource-relative controller routes ("products", "orders", ...) with a shared "api" prefix
    // applied once here, so the SPA's axios base + Vite dev proxy stay in sync with one setting.
    builder.Services.AddControllers(o => o.Conventions.Add(new RoutePrefixConvention("api")));
    builder.Services.ConfigureDefaultJsonOptions(); // Regira.Entities.Web.DependencyInjection
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<WebshopDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddEntityServices();

    builder.Services.AddCors(o => o.AddPolicy("Spa", policy => policy
        .SetIsOriginAllowed(origin => origin is "http://localhost:6181" or "http://127.0.0.1:6181")
        .AllowAnyHeader()
        .AllowAnyMethod()));

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseCors("Spa");
    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
        dbContext.Database.EnsureCreated();
        await DataSeeder.SeedAsync(app.Services);
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
