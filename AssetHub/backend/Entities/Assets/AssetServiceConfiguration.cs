using AssetHub.Api.Data;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Extensions;

namespace AssetHub.Api.Entities.Assets;

// Budget: complex 1/2
public static class AssetServiceConfiguration
{
    public static EntityServiceCollection<AppDbContext> AddAssets(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Asset, AssetSearchObject, AssetSortBy, AssetIncludes>(e =>
        {
            e.AddFilter<AssetQueryBuilder>();

            e.SortBy((query, sortBy) => sortBy switch
            {
                AssetSortBy.Title => query.OrderOrThenBy(x => x.Title),
                AssetSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                AssetSortBy.PurchaseDate => query.OrderOrThenBy(x => x.PurchaseDate),
                AssetSortBy.PurchaseDateDesc => query.OrderOrThenByDescending(x => x.PurchaseDate),
                AssetSortBy.Category => query.OrderOrThenBy(x => x.Category!.Title),
                AssetSortBy.Status => query.OrderOrThenBy(x => x.Status!.Title),
                AssetSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenByDescending(x => x.Created)
            });

            // Category/Status/Location/Supplier: cheap to-one refs shown on every inventory row -> unconditional.
            // Attachments/Warranties/MaintenanceRecords/Assignments: flag-gated, Details loads the OR of all of them.
            e.Includes((query, includes) =>
            {
                query = query
                    .Include(x => x.Category!)
                    .Include(x => x.Status!)
                    .Include(x => x.Location!)
                    .Include(x => x.Supplier!);

                if (includes?.HasFlag(AssetIncludes.Attachments) == true)
                {
                    query = query.Include(x => x.Attachments!.OrderBy(a => a.SortOrder));
                }
                if (includes?.HasFlag(AssetIncludes.Warranties) == true)
                {
                    query = query.Include(x => x.Warranties!);
                }
                if (includes?.HasFlag(AssetIncludes.MaintenanceRecords) == true)
                {
                    query = query.Include(x => x.MaintenanceRecords!);
                }
                if (includes?.HasFlag(AssetIncludes.Assignments) == true)
                {
                    query = query.Include(x => x.Assignments!.OrderByDescending(a => a.AssignedDate)).ThenInclude(a => a.Employee!);
                }

                return query.AsSplitQuery();
            });

            e.Related(x => x.Attachments, item => item.Attachments?.SetSortOrder());
            e.Related<AssetWarranty>(x => x.Warranties);
            e.Related<AssetMaintenanceRecord>(x => x.MaintenanceRecords);

            e.AddProcessor<AssetProcessor>();
            e.AddPrimer<AssetCodePrimer>();
        });
}
