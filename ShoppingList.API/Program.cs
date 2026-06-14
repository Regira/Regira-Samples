using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Serilog;
using ShoppingListApi.Data;
using ShoppingListApi.Extensions;
using ShoppingListApi.Seeding;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // Catch missing / mismatched entity-service registrations at startup instead of first request.
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

    // EF Core (SQLite) with Regira interceptors for primers, normalizers, and auto-truncate.
    builder.Services.AddDbContext<ShoppingListDbContext>((sp, options) =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
            .AddPrimerInterceptors(sp)
            .AddNormalizerInterceptors(sp)
            .AddAutoTruncateInterceptors());

    builder.Services.AddEntityServices(builder.Configuration);

    var app = builder.Build();

    // Create the SQLite database (no migrations for the disposable starter DB) and seed sample data.
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ShoppingListDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
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
