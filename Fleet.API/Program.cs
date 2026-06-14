using System.Text.Json.Serialization;
using Fleet.API.Data;
using Fleet.API.Extensions;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // Fail fast on DI misconfiguration (e.g. .For<>() / controller generic mismatches).
    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateOnBuild = true;
        o.ValidateScopes = true;
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
    builder.Services.AddOpenApi();

    // SQLite + Regira entity interceptors (primers, normalizers, auto-truncate).
    builder.Services.AddDbContext<FleetDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)
               .AddNormalizerInterceptors(sp)
               .AddAutoTruncateInterceptors());

    builder.Services.AddEntityServices(builder.Configuration);

    var app = builder.Build();

    // Create the SQLite database (disposable test infrastructure) and seed sample data.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FleetDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
    await FleetDbSeeder.SeedAsync(app.Services, app.Services.GetRequiredService<ILogger<Program>>());

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

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
