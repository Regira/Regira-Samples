using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.Models;
using QCredits.Api.Data;

namespace QCredits.Api.Entities.EmployeeCarryOvers;

public static class EmployeeCarryOverServiceConfiguration
{
    public static EntityServiceCollection<QCreditsDbContext> AddEmployeeCarryOvers(this IEntityServiceCollection<QCreditsDbContext> services)
        => services.For<EmployeeCarryOver, int, EmployeeCarryOverSearchObject>(e =>
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
                return query;
            });
            e.SortBy(query => query.OrderByDescending(x => x.Year));
            // Employee is a to-one shown on every row -> eager-load unconditionally
            e.Includes((query, includes) => query.Include(x => x.Employee!));
        });
}
