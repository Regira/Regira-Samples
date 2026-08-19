export interface EmployeeBalance {
    employeeId: number
    employeeName: string
    department?: string
    year: number
    annualCredits: number
    reservedCredits: number
    freelyAvailableCredits: number
    carriedOverCredits: number
    approvedCredits: number
    pendingCredits: number
    remainingCredits: number
    minBalance: number
}
