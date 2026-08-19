using Regira.Entities.Models;

namespace AssetHub.Api.Entities.Employees;

public record EmployeeSearchObject : SearchObject
{
    public string? Name { get; set; }
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
}
