namespace AssetHub.Api.Infrastructure.Security;

public static class Roles
{
    /// <summary>Manages reference data (categories, statuses, locations, suppliers) and user accounts</summary>
    public const string Admin = "Admin";
    /// <summary>Manages assets, employees, assignments and attachments</summary>
    public const string Manager = "Manager";

    public static readonly string[] All = [Admin, Manager];
}
