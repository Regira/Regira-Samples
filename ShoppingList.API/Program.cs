using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Serilog;
using ShoppingList.API.Data;
using ShoppingList.API.Extensions;
using ShoppingList.API.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // Fail fast on entity-service registration mismatches.
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

    // DbContext with Regira interceptors (primers, normalizers, auto-truncate).
    builder.Services.AddDbContext<ShoppingDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors());

    // Regira entity services + shopping-list domain entities.
    builder.Services.AddEntityServices();

    var app = builder.Build();

    // Create the SQLite database (disposable test infrastructure) and seed sample data.
    await SeedData.InitializeAsync(app.Services);

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
