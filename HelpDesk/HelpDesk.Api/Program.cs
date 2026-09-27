using HelpDesk.Api.Data;
using HelpDesk.Api.Data.Seeding;
using HelpDesk.Api.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));
    builder.AddServices();

    var app = builder.Build();

    // database + seed: between Build() and Run()
    using (var scope = app.Services.CreateScope())
    {
        Directory.CreateDirectory(Path.Combine(app.Environment.ContentRootPath, "App_Data"));
        var dbContext = scope.ServiceProvider.GetRequiredService<HelpDeskDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            dbContext.Database.EnsureDeleted(); // dotnet run -- --ResetDatabase=true
        dbContext.Database.EnsureCreated();
        if (app.Configuration.GetValue<bool>("Seed:Enabled"))
            await scope.ServiceProvider.SeedHelpDesk(app.Configuration);
    }

    app.UsePipeline();
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
