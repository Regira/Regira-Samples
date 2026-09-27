using Microsoft.EntityFrameworkCore;
using QCredits.Api.Data;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;

namespace QCredits.Api.Entities.CreditAllocations;

public static class CreditAllocationServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddCreditAllocations(this IEntityServiceCollection<AppDbContext> services)
        => services.For<CreditAllocation, int, CreditAllocationSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.EmployeeId?.Any() == true) query = query.Where(x => so.EmployeeId.Contains(x.EmployeeId));
                if (so.DepartmentId?.Any() == true) query = query.Where(x => so.DepartmentId.Contains(x.Employee!.DepartmentId));
                if (so.Year.HasValue) query = query.Where(x => x.Year == so.Year.Value);
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.Year)
                .ThenBy(x => x.Employee!.LastName).ThenBy(x => x.Employee!.FirstName));
            // the employee (and department) is shown on every list row
            e.Includes((query, _) => query.Include(x => x.Employee!).ThenInclude(x => x.Department));
            e.AddProcessor<CreditAllocationProcessor>();
            e.Prepare(async (item, db) =>
            {
                if (item.ReservedCredits > item.AnnualCredits)
                    throw new EntityInputException<CreditAllocation>("Invalid allocation")
                    {
                        InputErrors = { [nameof(CreditAllocation.ReservedCredits)] = "Reserved credits cannot exceed the annual budget." }
                    };
                if (item.ReservedUsed > item.ReservedCredits)
                    throw new EntityInputException<CreditAllocation>("Invalid allocation")
                    {
                        InputErrors = { [nameof(CreditAllocation.ReservedUsed)] = "Used reserved credits cannot exceed the reserved credits." }
                    };
                if (item.Id == 0)
                {
                    if (!await db.Employees.AnyAsync(x => x.Id == item.EmployeeId))
                        throw new EntityInputException<CreditAllocation>("Invalid allocation")
                        {
                            InputErrors = { [nameof(CreditAllocation.EmployeeId)] = "Unknown employee." }
                        };
                }
            });
        });
}
