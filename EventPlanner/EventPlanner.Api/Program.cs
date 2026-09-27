using EventPlanner.Api.Data.Seeding;
using EventPlanner.Api.Infrastructure;
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

    builder.Services
        .AddApi()
        .AddData(builder.Configuration)
        .AddSecurity(builder.Configuration)
        .AddEntityServices();

    var app = builder.Build();

    await app.InitializeDatabase();   // EnsureCreated + seed (between Build and Run)

    app.UseApi();
    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
