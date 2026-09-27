using Regira.Entities.EFcore.Extensions;
using Regira.Entities.QueryBuilders.Abstractions;

namespace AssetHub.Api.Entities.Assets;

public class AssetQueryBuilder : IFilteredQueryBuilder<Asset, int, AssetSearchObject>
{
    public IQueryable<Asset> Build(IQueryable<Asset> query, AssetSearchObject? so)
    {
        if (so == null) return query;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (so.CategoryId?.Any() == true) query = query.Where(x => so.CategoryId.Contains(x.CategoryId));
        if (so.StatusId?.Any() == true) query = query.Where(x => so.StatusId.Contains(x.StatusId));
        if (so.StatusKind?.Any() == true) query = query.Where(x => so.StatusKind.Contains(x.Status!.Kind));
        if (so.LocationId?.Any() == true) query = query.Where(x => x.LocationId.HasValue && so.LocationId.Contains(x.LocationId.Value));
        if (so.SupplierId?.Any() == true) query = query.Where(x => x.SupplierId.HasValue && so.SupplierId.Contains(x.SupplierId.Value));
        if (so.EmployeeId?.Any() == true) query = query.Where(x => x.CurrentEmployeeId.HasValue && so.EmployeeId.Contains(x.CurrentEmployeeId.Value));
        if (so.IsAssigned.HasValue) query = so.IsAssigned.Value ? query.Where(x => x.CurrentEmployeeId != null) : query.Where(x => x.CurrentEmployeeId == null);
        if (so.HasAttachment.HasValue) query = query.FilterHasAttachment(so.HasAttachment);
        if (so.UnderWarranty.HasValue)
            query = so.UnderWarranty.Value
                ? query.Where(x => x.Warranties!.Any(w => w.StartDate <= today && w.EndDate >= today))
                : query.Where(x => !x.Warranties!.Any(w => w.StartDate <= today && w.EndDate >= today));
        if (so.WarrantyExpiresBefore.HasValue)
        {
            var limit = so.WarrantyExpiresBefore.Value;
            query = query.Where(x => x.Warranties!.Any(w => w.EndDate >= today && w.EndDate <= limit));
        }
        if (so.MaintenanceDueBefore.HasValue)
        {
            var limit = so.MaintenanceDueBefore.Value;
            query = query.Where(x => x.MaintenanceRecords!.Any(m => m.NextDueDate != null && m.NextDueDate <= limit));
        }
        if (so.MinPurchaseDate.HasValue) query = query.Where(x => x.PurchaseDate >= so.MinPurchaseDate);
        if (so.MaxPurchaseDate.HasValue) query = query.Where(x => x.PurchaseDate <= so.MaxPurchaseDate);
        return query;
    }
}
