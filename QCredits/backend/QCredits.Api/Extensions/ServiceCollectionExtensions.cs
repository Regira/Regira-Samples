using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.Mapping.Mapster;
using QCredits.Api.Data;
using QCredits.Api.Entities.CreditPolicies;
using QCredits.Api.Entities.EmployeeCarryOvers;
using QCredits.Api.Entities.Employees;
using QCredits.Api.Entities.GroupTrainings;
using QCredits.Api.Entities.QCreditRequests;

namespace QCredits.Api.Extensions;

public static class ServiceCollectionExtensions
{
    // Entity budget tally (free tier = 5 simple + 2 complex):
    //   Employee            simple   1/5
    //   CreditPolicy         simple   2/5
    //   EmployeeCarryOver    simple   3/5
    //   GroupTraining        simple   4/5
    //   QCreditRequest       complex  1/2  (typed TSortBy/TIncludes)
    //   QCreditRequestItem   owned via e.Related() on QCreditRequest -> no slot
    // -> 4 simple / 1 complex -> fits free tier
    public static IServiceCollection AddEntityServices(this IServiceCollection services)
        => services
            .UseEntities<QCreditsDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
            })
            .AddEmployees()
            .AddCreditPolicies()
            .AddEmployeeCarryOvers()
            .AddGroupTrainings()
            .AddQCreditRequests();
}
