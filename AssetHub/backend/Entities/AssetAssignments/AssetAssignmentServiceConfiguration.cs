using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Models;

namespace AssetHub.Api.Entities.AssetAssignments;

// Budget: complex 2/2. Asset + Employee are unconditional to-one refs -- nothing needs per-request opt-in,
// so the built-in EntityIncludes (Default/All) is used rather than a dedicated [Flags] enum.
public static class AssetAssignmentServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddAssetAssignments(this IEntityServiceCollection<AppDbContext> services)
        => services.For<AssetAssignment, AssetAssignmentSearchObject, AssetAssignmentSortBy, EntityIncludes>(e =>
        {
            e.AddFilter<AssetAssignmentQueryBuilder>();

            e.SortBy((query, sortBy) => sortBy switch
            {
                AssetAssignmentSortBy.AssignedDate => query.OrderOrThenBy(x => x.AssignedDate),
                AssetAssignmentSortBy.ReturnedDate => query.OrderOrThenBy(x => x.ReturnedDate),
                AssetAssignmentSortBy.ReturnedDateDesc => query.OrderOrThenByDescending(x => x.ReturnedDate),
                _ => query.OrderOrThenByDescending(x => x.AssignedDate)
            });

            e.Includes((query, _) => query
                .Include(x => x.Asset!)
                .Include(x => x.Employee!));

            e.AddPrepper<AssetAssignmentPrepper>();
        });
}
