using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using Scalar.AspNetCore;
using Serilog;
using ShopMate.Api.Data;
using ShopMate.Api.Infrastructure;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

    // Keep controller routes resource-relative ([Route("shopping-lists")], ...); apply the shared "api"
    // prefix once here so it stays configurable and matches the SPA's dev proxy / IConfig.api.
    builder.Services.AddControllers(options => options.UseCentralRoutePrefix(new RouteAttribute("api")));
    builder.Services.ConfigureDefaultJsonOptions();

    builder.Services.AddOpenApi();

    builder.Services.AddDbContext<ShopMateDbContext>(options => options
        .UseSqlite(builder.Configuration.GetConnectionString("Default"))
        // ShoppingList is the aggregate root (IArchivable) over Article, its required dependent - this is
        // the expected/benign case the Regira.Entities soft-delete guidance calls out, so the net10.0
        // required-navigation query-filter warning is silenced here rather than left as noise.
        .ConfigureWarnings(w => w.Ignore(CoreEventId.PossibleIncorrectRequiredNavigationWithQueryFilterInteractionWarning)));

    builder.Services.AddEntityServices();

    builder.Services.AddCors(o => o.AddPolicy("Spa", policy => policy
        .SetIsOriginAllowed(origin => new Uri(origin).IsLoopback)
        .AllowAnyHeader()
        .AllowAnyMethod()));

    builder.Host.UseDefaultServiceProvider(o =>
    {
        o.ValidateOnBuild = true;
        o.ValidateScopes = true;
    });

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    // The BasicApi template's UseHttpsRedirection() would 307 the SPA dev server, which talks HTTP-only.
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("Spa");
    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ShopMateDbContext>();
        dbContext.Database.EnsureCreated();
        await ShopMate.Api.Data.Seeding.DataSeeder.SeedAsync(scope.ServiceProvider);
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
