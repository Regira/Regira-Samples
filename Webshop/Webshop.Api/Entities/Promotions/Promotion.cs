using System.ComponentModel.DataAnnotations;
using Regira.Entities.Models.Abstractions;

namespace Webshop.Api.Entities.Promotions;

/// <summary>A promotional banner shown on the storefront (hero carousel and strips)</summary>
public class Promotion : IEntityWithSerial, IHasTimestamps, IHasTitle, IHasStartEndDate, ISortable
{
    public int Id { get; set; }
    [Required, MaxLength(96)] public string Title { get; set; } = null!;
    [MaxLength(256)] public string? Subtitle { get; set; }
    /// <summary>Short badge text, e.g. "-30%" or "NEW"</summary>
    [MaxLength(24)] public string? Badge { get; set; }
    [MaxLength(48)] public string? CtaLabel { get; set; }
    /// <summary>Storefront route the call-to-action navigates to, e.g. "/shop?onSale=true"</summary>
    [MaxLength(256)] public string? CtaLink { get; set; }
    /// <summary>Visual theme key used by the storefront (sunset, ocean, forest, berry, midnight)</summary>
    [MaxLength(24)] public string? Theme { get; set; }
    [MaxLength(64)] public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int SortOrder { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
