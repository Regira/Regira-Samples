using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Extensions;

namespace QCredits.Api.Entities.CreditRequests;

public static class CreditRequestServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddCreditRequests(this IEntityServiceCollection<AppDbContext> services)
        => services.For<CreditRequest, int, CreditRequestSearchObject, CreditRequestSortBy, CreditRequestIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.EmployeeId?.Any() == true) query = query.Where(x => so.EmployeeId.Contains(x.EmployeeId));
                if (so.DepartmentId?.Any() == true) query = query.Where(x => so.DepartmentId.Contains(x.Employee!.DepartmentId));
                if (so.Year.HasValue) query = query.Where(x => x.Year == so.Year.Value);
                if (so.Status?.Any() == true) query = query.Where(x => so.Status.Contains(x.Status));
                if (so.ActivityType?.Any() == true) query = query.Where(x => x.Items!.Any(i => so.ActivityType.Contains(i.Type)));
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                CreditRequestSortBy.Oldest => query.OrderOrThenBy(x => x.Created),
                CreditRequestSortBy.SubmittedAt => query.OrderOrThenBy(x => x.SubmittedAt),
                CreditRequestSortBy.SubmittedAtDesc => query.OrderOrThenByDescending(x => x.SubmittedAt),
                CreditRequestSortBy.CreditsDesc => query.OrderOrThenByDescending(x => x.TotalCredits),
                CreditRequestSortBy.Credits => query.OrderOrThenBy(x => x.TotalCredits),
                CreditRequestSortBy.Employee => query.OrderOrThenBy(x => x.Employee!.LastName).OrderOrThenBy(x => x.Employee!.FirstName),
                CreditRequestSortBy.Title => query.OrderOrThenBy(x => x.Title),
                CreditRequestSortBy.Status => query.OrderOrThenBy(x => x.Status),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });
            e.Includes((query, includes) =>
            {
                // employee + department are shown on every row: unconditional
                query = query.Include(x => x.Employee!).ThenInclude(x => x.Department);
                if (includes?.HasFlag(CreditRequestIncludes.Items) == true)
                    query = query.Include(x => x.Items!.OrderBy(i => i.SortOrder));
                return query;
            });
            e.Related(x => x.Items, item => item.Items?.SetSortOrder());
            e.Prepare(async (item, db) =>
            {
                // totals are server-computed; null = items not sent -> recompute from the persisted rows
                if (item.Items == null)
                {
                    if (item.Id > 0)
                    {
                        var rows = await db.CreditRequestItems.AsNoTracking()
                            .Where(x => x.CreditRequestId == item.Id)
                            .Select(x => new { x.Credits, x.Cost })
                            .ToListAsync();
                        item.TotalCredits = rows.Sum(x => x.Credits);
                        item.TotalCost = rows.Sum(x => x.Cost);
                    }
                    return;
                }
                item.TotalCredits = item.Items.Sum(x => x.Credits);
                item.TotalCost = item.Items.Sum(x => x.Cost);
            });
            e.AddPrepper<CreditRequestGuard>();
        });
}
