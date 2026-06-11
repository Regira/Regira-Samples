using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Regira.DAL.EFcore.Services;
using Regira.Entities.EFcore.Normalizing;
using Regira.Entities.EFcore.Primers;
using Scalar.AspNetCore;
using Webshop.API.Data;
using Webshop.API.Extensions;
using Webshop.API.Seeding;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<WebshopDbContext>((sp, options) =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default"))
           .AddPrimerInterceptors(sp)
           .AddNormalizerInterceptors(sp)
           .AddAutoTruncateInterceptors());

builder.Services.AddEntityServices(builder.Configuration);

builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WebshopDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
