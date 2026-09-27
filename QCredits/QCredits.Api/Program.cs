using Microsoft.AspNetCore.Authentication.JwtBearer;
using QCredits.Api.Data;
using QCredits.Api.Data.Seeding;
using QCredits.Api.Infrastructure;
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

    builder.Services
        .AddApi(builder.Configuration)
        .AddDatabase(builder.Configuration)
        .AddAuth(builder.Configuration)
        .AddEntityServices();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            dbContext.Database.EnsureDeleted(); // dotnet run -- --ResetDatabase=true
        dbContext.Database.EnsureCreated();
        if (app.Configuration.GetValue<bool>("Seed:Enabled"))
            await scope.ServiceProvider.GetRequiredService<DataSeeder>().Seed();
    }

    app.MapOpenApi().AllowAnonymous();
    app.MapScalarApiReference(options =>
    {
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecuritySchemes = [JwtBearerDefaults.AuthenticationScheme]
        };
    }).AllowAnonymous();

    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();

    app.UseCors();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers().RequireAuthorization();

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
