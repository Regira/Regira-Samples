namespace AssetHub.Api.Entities.Assets;

// Plain (non-flags) sort enum -- a request may carry several values, applied left to right.
public enum AssetSortBy
{
    Title,
    TitleDesc,
    Created,
    CreatedDesc,
    PurchaseDate,
    PurchaseDateDesc,
    Category,
    Status
}
