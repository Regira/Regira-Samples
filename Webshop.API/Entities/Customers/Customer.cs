using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Webshop.API.Entities.Customers;

public class Customer : IEntity<Guid>, IHasTimestamps, IHasNormalizedContent
{
    public Guid Id { get; set; }
    [Required, MaxLength(256)] public string Name { get; set; } = null!;
    [Required, MaxLength(256)] public string Email { get; set; } = null!;
    [MaxLength(64)] public string? Phone { get; set; }
    [MaxLength(512), Normalized(SourceProperties = [nameof(Name), nameof(Email)])]
    public string? NormalizedContent { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
    public ICollection<Orders.Order>? Orders { get; set; }
}
