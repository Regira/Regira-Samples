// Mirrors Fleet.Api.Controllers.Dashboard*Dto (read-only cross-entity aggregates -- see
// Regira.Entities -> entities.patterns -> Cross-entity aggregates & report endpoints).

export interface DashboardSummary {
    totalVehicles: number
    activeVehicles: number
    vehiclesInMaintenance: number
    outOfServiceVehicles: number

    openInterventions: number
    scheduledInterventions: number
    inProgressInterventions: number
    overdueInterventions: number
    completedThisMonth: number

    totalSuppliers: number
    activeSuppliers: number

    totalSpend: number
    spendThisMonth: number

    draftInvoices: number
    overdueInvoices: number
    outstandingAmount: number
}

export interface MonthlySpend {
    year: number
    month: number
    total: number
}

export interface StatusCount {
    status: string
    count: number
}

export interface TopSupplier {
    supplierId: number
    supplierName?: string
    interventionCount: number
    totalSpend: number
}
