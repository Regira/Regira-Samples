using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.Common;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Interventions;

/// <summary>
/// A maintenance operation performed on a vehicle by a supplier, of a given intervention type, and
/// optionally billed on a supplier invoice.
/// </summary>
public class Intervention : IEntityWithSerial, IHasTimestamps, IHasCode, IHasDescription, IHasNormalizedContent
{
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string? Code { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    /// <summary>Invoice this intervention is billed on, when invoiced.</summary>
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public InterventionStatus Status { get; set; }

    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    /// <summary>Vehicle odometer reading (km) at the time of service.</summary>
    public int MileageAtService { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }

    public decimal Cost { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = new[] { nameof(Code), nameof(Description) })]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
