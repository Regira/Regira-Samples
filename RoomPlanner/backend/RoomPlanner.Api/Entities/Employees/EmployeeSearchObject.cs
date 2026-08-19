using Regira.Entities.Models;

namespace RoomPlanner.Api.Entities.Employees;

public record EmployeeSearchObject : SearchObject
{
    public string? Department { get; set; }
    public bool? IsActive { get; set; }
}
