using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace AssetHub.Api.Entities.Suppliers;

public class Supplier : IEntityWithSerial, IHasTitle, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }
    [Required, MaxLength(96)] public string? Title { get; set; }
    [MaxLength(96)] public string? ContactName { get; set; }
    [MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Website { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }
    [MaxLength(1024), Normalized(SourceProperties = [nameof(Title), nameof(ContactName), nameof(Email)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public record SupplierSearchObject : SearchObject;

public class SupplierDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}

public class SupplierInputDto
{
    public int Id { get; set; }
    [Required, MaxLength(96)] public string? Title { get; set; }
    [MaxLength(96)] public string? ContactName { get; set; }
    [MaxLength(128), EmailAddress] public string? Email { get; set; }
    [MaxLength(32)] public string? Phone { get; set; }
    [MaxLength(256)] public string? Website { get; set; }
    [MaxLength(256)] public string? Address { get; set; }
    [MaxLength(1024)] public string? Notes { get; set; }
}
