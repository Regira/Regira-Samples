using AssetHub.Api.Data;
using AssetHub.Api.Entities.Assets;
using AssetHub.Api.Entities.AssetStatuses;
using AssetHub.Api.Entities.Categories;
using AssetHub.Api.Entities.Employees;
using AssetHub.Api.Entities.Locations;
using AssetHub.Api.Entities.Suppliers;
using AssetHub.Api.Services;
using Microsoft.EntityFrameworkCore;
using Regira.Entities.DependencyInjection.Attachments;
using Regira.Entities.DependencyInjection.Extensions;
using Regira.Entities.DependencyInjection.ServiceCollections;
using Regira.Entities.DependencyInjection.ServiceCollections.Abstractions;
using Regira.Entities.EFcore.Attachments;
using Regira.Entities.EFcore.Extensions;
using Regira.Entities.Mapping.Mapster;
using Regira.Entities.Web.Attachments.DependencyInjection;
using Regira.IO.Storage.FileSystem;

namespace AssetHub.Api.Extensions;

/*
 * Regira Entities budget (free tier = 5 simple + 2 complex)
 * | Entity             | Classification                          | Running tally |
 * |--------------------|-----------------------------------------|---------------|
 * | Category           | simple                                  | 1/5 simple    |
 * | AssetStatus        | simple                                  | 2/5 simple    |
 * | Location           | simple                                  | 3/5 simple    |
 * | Supplier           | simple                                  | 4/5 simple    |
 * | AssetAttachment    | simple (HasAttachments join)            | 5/5 simple    |
 * | Employee           | complex (typed sort + includes)         | 1/2 complex   |
 * | Asset              | complex (typed sort + includes)         | 2/2 complex   |
 * | AssetAssignment    | owned child via Related() - no slot     | -             |
 * | Warranty           | owned child via Related() - no slot     | -             |
 * | MaintenanceRecord  | owned child via Related() - no slot     | -             |
 * | Attachment         | shared base via WithAttachments - free  | -             |
 * -> 5 simple / 2 complex -> fits free
 */
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<WorkflowContext>();
        services.AddScoped<AssetWorkflowService>();
        services.AddHttpContextAccessor();

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), configuration["Storage:UploadsFolder"] ?? "uploads");

        return services
            .UseEntities<AppDbContext>(options =>
            {
                options.UseDefaults();
                options.UseMapsterMapping();
                options.UseAttachmentUris();
                options.DefaultPageSize = 25;
                options.MaxPageSize = 1000;
            })
            .WithAttachments(_ => new BinaryFileService(new FileSystemOptions { RootFolder = uploadsFolder }))
            .AddCategories()
            .AddAssetStatuses()
            .AddLocations()
            .AddSuppliers()
            .AddEmployees()
            .AddAssets();
    }

    public static EntityServiceCollection<AppDbContext> AddCategories(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Category, int, CategorySearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });

    public static EntityServiceCollection<AppDbContext> AddAssetStatuses(this IEntityServiceCollection<AppDbContext> services)
        => services.For<AssetStatus, int, AssetStatusSearchObject>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so?.Kind?.Any() == true) query = query.Where(x => so.Kind.Contains(x.Kind));
                // AssetStatus has no NormalizedContent (a handful of rows), so ?q= is matched on the title here
                if (!string.IsNullOrWhiteSpace(so?.Q)) query = query.Where(x => EF.Functions.Like(x.Title!, $"%{so.Q.Trim()}%"));
                return query;
            });
            e.SortBy(query => query.OrderBy(x => x.SortOrder).ThenBy(x => x.Title));
        });

    public static EntityServiceCollection<AppDbContext> AddLocations(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Location, int, LocationSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });

    public static EntityServiceCollection<AppDbContext> AddSuppliers(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Supplier, int, SupplierSearchObject>(e =>
        {
            e.SortBy(query => query.OrderBy(x => x.Title));
        });

    public static EntityServiceCollection<AppDbContext> AddEmployees(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Employee, EmployeeSearchObject, EmployeeSortBy, EmployeeIncludes>(e =>
        {
            e.Filter((query, so) =>
            {
                if (so == null) return query;
                if (so.LocationId?.Any() == true) query = query.Where(x => x.LocationId.HasValue && so.LocationId.Contains(x.LocationId.Value));
                if (so.Department?.Any() == true) query = query.Where(x => x.Department != null && so.Department.Contains(x.Department));
                if (so.IsActive.HasValue) query = query.Where(x => x.IsActive == so.IsActive.Value);
                if (so.HasAssets.HasValue)
                    query = so.HasAssets.Value
                        ? query.Where(x => x.Assignments!.Any(a => a.ReturnedOn == null))
                        : query.Where(x => !x.Assignments!.Any(a => a.ReturnedOn == null));
                return query;
            });
            e.SortBy((query, sortBy) => sortBy switch
            {
                EmployeeSortBy.LastNameDesc => query.OrderOrThenByDescending(x => x.LastName).ThenByDescending(x => x.FirstName),
                EmployeeSortBy.Code => query.OrderOrThenBy(x => x.Code),
                EmployeeSortBy.CodeDesc => query.OrderOrThenByDescending(x => x.Code),
                EmployeeSortBy.Department => query.OrderOrThenBy(x => x.Department).ThenBy(x => x.LastName),
                EmployeeSortBy.DepartmentDesc => query.OrderOrThenByDescending(x => x.Department).ThenBy(x => x.LastName),
                EmployeeSortBy.HireDate => query.OrderOrThenBy(x => x.HireDate),
                EmployeeSortBy.HireDateDesc => query.OrderOrThenByDescending(x => x.HireDate),
                EmployeeSortBy.Created => query.OrderOrThenBy(x => x.Created),
                EmployeeSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                _ => query.OrderOrThenBy(x => x.LastName).ThenBy(x => x.FirstName)
            });
            e.Includes((query, includes) =>
            {
                query = query.Include(x => x.Location);
                if (includes?.HasFlag(EmployeeIncludes.Assignments) == true)
                {
                    query = query
                        .Include(x => x.Assignments!.OrderByDescending(a => a.AssignedOn))
                            .ThenInclude(a => a.Asset!).ThenInclude(a => a.Category)
                        .Include(x => x.Assignments!)
                            .ThenInclude(a => a.Asset!).ThenInclude(a => a.Status)
                        .AsSplitQuery();
                }
                return query;
            });
            e.AddProcessor<EmployeeProcessor>();
        });

    public static EntityServiceCollection<AppDbContext> AddAssets(this IEntityServiceCollection<AppDbContext> services)
        => services.For<Asset, AssetSearchObject, AssetSortBy, AssetIncludes>(e =>
        {
            e.AddFilter<AssetQueryBuilder>();
            e.SortBy((query, sortBy) => sortBy switch
            {
                AssetSortBy.Code => query.OrderOrThenBy(x => x.Code),
                AssetSortBy.CodeDesc => query.OrderOrThenByDescending(x => x.Code),
                AssetSortBy.Title => query.OrderOrThenBy(x => x.Title),
                AssetSortBy.TitleDesc => query.OrderOrThenByDescending(x => x.Title),
                AssetSortBy.PurchaseDate => query.OrderOrThenBy(x => x.PurchaseDate),
                AssetSortBy.PurchaseDateDesc => query.OrderOrThenByDescending(x => x.PurchaseDate),
                AssetSortBy.PurchasePrice => query.OrderOrThenBy(x => x.PurchasePrice),
                AssetSortBy.PurchasePriceDesc => query.OrderOrThenByDescending(x => x.PurchasePrice),
                AssetSortBy.Created => query.OrderOrThenBy(x => x.Created),
                AssetSortBy.CreatedDesc => query.OrderOrThenByDescending(x => x.Created),
                AssetSortBy.LastModified => query.OrderOrThenBy(x => x.LastModified),
                AssetSortBy.LastModifiedDesc => query.OrderOrThenByDescending(x => x.LastModified),
                _ => query.OrderOrThenByDescending(x => x.Id)
            });
            e.Includes((query, includes) =>
            {
                // to-one references shown on every list row: unconditional
                query = query
                    .Include(x => x.Category)
                    .Include(x => x.Status)
                    .Include(x => x.Location)
                    .Include(x => x.Supplier)
                    .Include(x => x.CurrentEmployee);
                // collections: flag-gated (Details loads them all)
                if (includes?.HasFlag(AssetIncludes.Assignments) == true)
                    query = query.Include(x => x.Assignments!.OrderByDescending(a => a.AssignedOn)).ThenInclude(a => a.Employee);
                if (includes?.HasFlag(AssetIncludes.Warranties) == true)
                    query = query.Include(x => x.Warranties!.OrderByDescending(w => w.EndDate));
                if (includes?.HasFlag(AssetIncludes.MaintenanceRecords) == true)
                    query = query.Include(x => x.MaintenanceRecords!.OrderByDescending(m => m.Date));
                if (includes?.HasFlag(AssetIncludes.Attachments) == true)
                    query = query.Include(x => x.Attachments!.OrderBy(a => a.SortOrder)).ThenInclude(a => a.Attachment);
                return query.AsSplitQuery();
            });
            e.Related(x => x.Warranties);
            e.Related(x => x.MaintenanceRecords);
            // written only by AssetWorkflowService: AssetInputDto leaves the collection out (null = untouched)
            e.Related(x => x.Assignments);
            e.AddPrepper<AssetPrepper>();
            e.HasAttachments<AppDbContext, Asset, AssetAttachment>(x => x.Attachments);
        });
}
