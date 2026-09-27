using System.ComponentModel.DataAnnotations;
using QCredits.Api.Entities.Employees;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace QCredits.Api.Entities.Departments;

public class Department : IEntityWithSerial, IHasTitle, IHasDescription, IHasCode, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string? Title { get; set; }
    [MaxLength(10)] public string? Code { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(Code), nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public ICollection<Employee>? Employees { get; set; }
}

public class DepartmentDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
}

public class DepartmentInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string? Title { get; set; }
    [MaxLength(10)] public string? Code { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
}
