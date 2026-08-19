using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Scalar.AspNetCore;
using Serilog;
using QCredits.Api.Data;
using QCredits.Api.Extensions;

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

    builder.Services.AddControllers();
    builder.Services.ConfigureDefaultJsonOptions(); // cycles/nulls/enum-names on MVC + Http.Json options

    builder.Services.AddDbContext<QCreditsDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddEntityServices();
    builder.Services.AddScoped<QCredits.Api.Balances.BalanceCalculator>();

    builder.Services.AddOpenApi();

    // CORS for the SPA dev origin (http://localhost:6151)
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy => policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod());
    });

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    app.UseCors();

    app.UseAuthorization();

    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<QCreditsDbContext>();
        dbContext.Database.EnsureCreated();
    }
    await SeedData.SeedAsync(app.Services);

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
