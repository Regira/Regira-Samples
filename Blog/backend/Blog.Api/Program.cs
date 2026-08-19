using Blog.Api.Data;
using Blog.Api.Extensions;
using Blog.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
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

    builder.Services.AddControllers();
    builder.Services.ConfigureDefaultJsonOptions();
    builder.Services.AddOpenApi();

    builder.Services.AddCors(o => o.AddPolicy("Spa", p => p
        .SetIsOriginAllowed(_ => true)
        .AllowAnyHeader()
        .AllowAnyMethod()));

    builder.Services.AddDbContext<BlogDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

    builder.Services.AddEntityServices();

    var app = builder.Build();

    app.MapOpenApi();
    app.MapScalarApiReference();

    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("Spa");
    app.UseAuthorization();
    app.MapControllers();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        dbContext.Database.EnsureCreated();
        await SeedData.SeedAsync(dbContext, scope.ServiceProvider);
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
