namespace AssetHub.Api.Entities.Assets;

// Category/Status/Location/Supplier are cheap to-one refs, loaded unconditionally in the query builder.
// These flags gate the heavier owned collections + the assignment history back-reference.
[Flags]
public enum AssetIncludes
{
    Default = 0,
    Attachments = 1 << 0,
    Warranties = 1 << 1,
    MaintenanceRecords = 1 << 2,
    Assignments = 1 << 3,
    All = Attachments | Warranties | MaintenanceRecords | Assignments
}
