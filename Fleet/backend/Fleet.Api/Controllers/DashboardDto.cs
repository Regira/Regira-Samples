namespace Fleet.Api.Controllers;

public class DashboardSummaryDto
{
    public int TotalVehicles { get; set; }
    public int ActiveVehicles { get; set; }
    public int VehiclesInMaintenance { get; set; }
    public int OutOfServiceVehicles { get; set; }

    public int OpenInterventions { get; set; }
    public int ScheduledInterventions { get; set; }
    public int InProgressInterventions { get; set; }
    public int OverdueInterventions { get; set; }
    public int CompletedThisMonth { get; set; }

    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }

    public decimal TotalSpend { get; set; }
    public decimal SpendThisMonth { get; set; }

    public int DraftInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class MonthlySpendDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = null!;
    public int Count { get; set; }
}

public class TopSupplierDto
{
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int InterventionCount { get; set; }
    public decimal TotalSpend { get; set; }
}
