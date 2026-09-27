using Blog.Api.Data;
using Blog.Api.Data.Seeding;
using Blog.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.Web.DependencyInjection;
using Regira.Web.Routing;
using Scalar.AspNetCore;

namespace Blog.Api.Infrastructure;

public static class HostingExtensions
{
    public static IServiceCollection AddBlogServices(this IServiceCollection services, IConfiguration configuration)
    {
        // every controller is served under /api (the SPA's axios base + Vite proxy agree with it)
        services.AddControllers(o => o.UseCentralRoutePrefix(new RouteAttribute("api")));
        // cycles/nulls/enum names/UTC dates on MVC + Http.Json options, plus the 400/409 entity-exception filter
        services.ConfigureDefaultJsonOptions();
        services.AddOpenApi();

        services.AddDbContext<BlogDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default")));

        services.AddEntityServices();
        services.AddScoped<BlogSeeder>();

        return services;
    }

    public static async Task InitializeDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
        if (app.Environment.IsDevelopment() && app.Configuration.GetValue<bool>("ResetDatabase"))
            await dbContext.Database.EnsureDeletedAsync(); // dotnet run -- --ResetDatabase=true
        await dbContext.Database.EnsureCreatedAsync();

        if (app.Configuration.GetValue("Seeding:Enabled", true))
        {
            var seeder = scope.ServiceProvider.GetRequiredService<BlogSeeder>();
            await seeder.Seed(app.Configuration.GetValue("Seeding:PostCount", 500));
        }
    }

    public static WebApplication UseBlogPipeline(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference();

        // the Vite dev server proxies plain HTTP to this API - only redirect outside Development
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        return app;
    }
}
