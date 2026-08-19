using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Regira.Entities.DependencyInjection.Preppers;
using Regira.Entities.DependencyInjection.Primers;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using QCredits.Api.Data;

namespace QCredits.Api.Entities.QCreditRequests;

public static class QCreditRequestServiceConfiguration
{
    public static EntityServiceCollection<QCreditsDbContext> AddQCreditRequests(this IEntityServiceCollection<QCreditsDbContext> services)
        => services.For<QCreditRequest, QCreditRequestSearchObject, EntitySortBy, QCreditRequestIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.EmployeeId?.Any() == true)
                {
                    query = query.Where(x => so.EmployeeId.Contains(x.EmployeeId));
                }
                if (so?.Year?.Any() == true)
                {
                    query = query.Where(x => so.Year.Contains(x.Year));
                }
                if (so?.Status?.Any() == true)
                {
                    query = query.Where(x => so.Status.Contains(x.Status));
                }
                return query;
            });
            e.SortBy((query, sortBy) => query.OrderByDescending(x => x.SubmittedDate));
            e.Includes((query, includes) =>
            {
                // to-one references shown on every row -> eager-load unconditionally
                query = query.Include(x => x.Employee!).Include(x => x.Approver!);
                if (includes?.HasFlag(QCreditRequestIncludes.Items) == true)
                {
                    query = query.Include(x => x.Items!).AsSplitQuery();
                }
                return query;
            });
            e.Related<QCreditRequestItem>(x => x.Items);

            e.Prepare(async (request, dbContext) =>
            {
                // three-way on the incoming collection: null = not sent (status-only change via workflow
                // controller), recompute from the persisted rows; [] / populated = recompute from what was sent
                if (request.Items == null)
                {
                    request.TotalCredits = request.Id > 0
                        ? await dbContext.Set<QCreditRequestItem>().AsNoTracking()
                            .Where(x => x.RequestId == request.Id)
                            .SumAsync(x => x.Credits)
                        : 0m;
                }
                else
                {
                    request.TotalCredits = request.Items.Sum(x => x.Credits);
                }
            });

            e.AddPrimer<QCreditRequestStatusPrimer>();
            e.AddScoped<RequestWorkflowContext>();
        });
}
