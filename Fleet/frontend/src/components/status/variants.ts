// Bootstrap contextual color per status value -- one place to keep every status badge consistent.

export const vehicleStatusVariants: Record<string, string> = {
    Active: "success",
    InMaintenance: "warning",
    OutOfService: "danger",
    Retired: "secondary",
}

export const interventionStatusVariants: Record<string, string> = {
    Scheduled: "info",
    InProgress: "primary",
    Completed: "success",
    Cancelled: "secondary",
}

export const invoiceStatusVariants: Record<string, string> = {
    Draft: "secondary",
    Sent: "info",
    Paid: "success",
    Overdue: "danger",
    Cancelled: "dark",
}
