namespace Fleet.Api.Entities.Vehicles;

public class VehicleDto
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = null!;
    public string Brand { get; set; } = null!;
    public string Model { get; set; } = null!;
    public VehicleType Type { get; set; }
    public VehicleStatus Status { get; set; }
    public int Year { get; set; }
    public int Mileage { get; set; }
    public string? Vin { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime Created { get; set; }
    public DateTime? LastModified { get; set; }
}
