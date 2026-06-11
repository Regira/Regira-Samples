using System.Text.Json.Serialization;
using Fleet.API.Data;
using Fleet.API.Infrastructure;
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

    // Fail fast on mismatched / missing entity service registrations at startup.
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
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<FleetDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
               .AddPrimerInterceptors(sp)
               .AddNormalizerInterceptors(sp)
               .AddAutoTruncateInterceptors());

    builder.Services.AddFleetEntityServices();

    var app = builder.Build();

    // Create the disposable SQLite database and seed sample data.
    using (var scope = app.Services.CreateScope())
    {
        await DataSeeder.SeedAsync(scope.ServiceProvider);
    }

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
