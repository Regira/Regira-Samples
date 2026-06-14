using System.ComponentModel.DataAnnotations;
using Fleet.API.Entities.InterventionTypes;
using Fleet.API.Entities.Invoices;
using Fleet.API.Entities.Suppliers;
using Fleet.API.Entities.Vehicles;
using Regira.Entities.Models.Abstractions;
using Regira.Normalizing;

namespace Fleet.API.Entities.Interventions;

/// <summary>
/// A maintenance operation performed on a <see cref="Vehicle"/> by a <see cref="Supplier"/>,
/// of a given <see cref="InterventionType"/>, optionally billed through an <see cref="Invoice"/>.
/// </summary>
public class Intervention : IEntityWithSerial, IHasDescription, IHasTimestamps, IHasNormalizedContent
{
    public int Id { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int InterventionTypeId { get; set; }
    public InterventionType? InterventionType { get; set; }

    /// <summary>Optional link to the invoice that bills this intervention.</summary>
    public int? InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }

    public InterventionStatus Status { get; set; }

    public decimal Cost { get; set; }

    /// <summary>Vehicle odometer reading (km) at the moment of the intervention.</summary>
    public int? MileageAtService { get; set; }

    [MaxLength(1024)]
    public string? Description { get; set; }

    [MaxLength(1024), Normalized(SourceProperties = [nameof(Description)])]
    public string? NormalizedContent { get; set; }

    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
