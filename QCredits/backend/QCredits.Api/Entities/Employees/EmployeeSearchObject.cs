using Regira.Entities.Models;

namespace QCredits.Api.Entities.Employees;

public record EmployeeSearchObject : SearchObject
{
    public ICollection<string>? Department { get; set; }
    public ICollection<EmployeeRole>? Role { get; set; }
    public bool? IsActive { get; set; }
}
