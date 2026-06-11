namespace Fleet.API.Entities.Common;

/// <summary>Category of a fleet vehicle.</summary>
public enum VehicleType
{
    Car = 0,
    Van = 1,
    Truck = 2,
    Bus = 3,
    Motorcycle = 4,
    Trailer = 5
}

/// <summary>Lifecycle state of a maintenance intervention.</summary>
public enum InterventionStatus
{
    Planned = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3
}

/// <summary>State of a supplier invoice.</summary>
public enum InvoiceStatus
{
    Draft = 0,
    Sent = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4
}
