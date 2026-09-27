using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using QCredits.Api.Data.Seeding;
using QCredits.Api.Entities.CreditAllocations;
using QCredits.Api.Entities.CreditRequests;
using QCredits.Api.Entities.CreditYears;
using QCredits.Api.Entities.Departments;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Infrastructure.Security;
using QCredits.Api.Services;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.DependencyInjection.QueryBuilders;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Web.DependencyInjection;
using Regira.Security.Authentication.Core.Models;
using Regira.Security.Authentication.Jwt.Extensions;
using Regira.Security.Authentication.Web.OpenApi.Transformers;
using Regira.Web.Routing;

namespace QCredits.Api.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers(o =>
        {
            o.UseCentralRoutePrefix(new RouteAttribute("api"));
            o.Filters.Add<WriteAuthorizationFilter>();
        });
        services.ConfigureDefaultJsonOptions();
        services.AddHttpContextAccessor();

        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer<AuthenticationSchemeDocumentTransformer>();
            options.AddOperationTransformer<SecurityRequirementOperationTransformer>();
        });

        var origins = configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        services.AddCors(o => o.AddDefaultPolicy(p => p
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
        => services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Default"),
                o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<AppUser>(o =>
            {
                // the JWT scheme validates roles against "role"
                o.ClaimsIdentity.RoleClaimType = RegiraClaimTypes.Role;
                o.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddJwtAuthentication(o => configuration.GetSection(AuthenticationSections.Jwt).Bind(o));
        services.AddTransient<IEmailSender, DevEmailSender>();
        return services;
    }

    public static IServiceCollection AddEntityServices(this IServiceCollection services)
    {
        services.AddScoped<WorkflowContext>();
        services.AddScoped<AccessScope>();
        services.AddScoped<BalanceService>();
        services.AddScoped<DataSeeder>();

        // Free-tier budget (5 simple + 2 complex):
        // | Entity                   | Classification                         | Tally        |
        // | Department               | simple                                 | 1/5 simple   |
        // | Employee                 | simple (SearchObject)                  | 2/5 simple   |
        // | CreditYear               | simple                                 | 3/5 simple   |
        // | CreditAllocation         | simple (SearchObject)                  | 4/5 simple   |
        // | CreditRequest            | complex (typed SortBy/Includes)        | 1/2 complex  |
        // | CreditRequestItem        | owned child via Related() - no slot    | -            |
        // | GroupTraining            | complex (typed SortBy/Includes)        | 2/2 complex  |
        // | GroupTrainingParticipant | owned join via Related() - no slot     | -            |
        return services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.AddGlobalFilterQueryBuilder<EmployeeScopeFilter>();
                options.AddGlobalFilterQueryBuilder<CreditAllocationScopeFilter>();
                options.AddGlobalFilterQueryBuilder<CreditRequestScopeFilter>();
            })
            .AddDepartments()
            .AddEmployees()
            .AddCreditYears()
            .AddCreditAllocations()
            .AddCreditRequests()
            .AddGroupTrainings();
    }
}
