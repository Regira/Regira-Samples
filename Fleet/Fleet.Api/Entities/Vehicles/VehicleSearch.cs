using Regira.Entities.Models;

namespace Fleet.Api.Entities.Vehicles;

public record VehicleSearchObject : SearchObject
{
    public ICollection<VehicleType>? VehicleType { get; set; }
    public ICollection<FuelType>? FuelType { get; set; }
    public ICollection<VehicleStatus>? Status { get; set; }
    public string? Department { get; set; }
    /// <summary>Vehicles whose next service is due on or before this date.</summary>
    public DateOnly? ServiceDueBefore { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
}

public enum VehicleSortBy
{
    Default = 0,
    LicensePlate, LicensePlateDesc,
    Make, MakeDesc,
    Year, YearDesc,
    Mileage, MileageDesc,
    NextServiceDate, NextServiceDateDesc,
    Status
}
