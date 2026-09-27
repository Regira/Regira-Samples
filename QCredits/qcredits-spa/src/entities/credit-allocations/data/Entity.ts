import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"

/** Yearly QCredit budget of one employee. Free = annual - reserved + carried over; remaining = free - used. */
export class CreditAllocation extends EntityBase {
    id: number = 0
    employeeId?: number
    employee?: Employee // eager-loaded by the API on every row
    year: number = new Date().getFullYear()
    annualCredits = 20
    reservedCredits = 5
    reservedUsed = 0
    carriedOver = 0
    minBalance = -10
    notes?: string

    // computed by the API (read-only)
    freeCredits = 0
    usedCredits = 0
    pendingCredits = 0
    remainingCredits = 0

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        const name = this.employee ? `${this.employee.firstName ?? ""} ${this.employee.lastName ?? ""}`.trim() : undefined
        return name ? `${name} - ${this.year}` : String(this.year)
    }
}

export const Entity = CreditAllocation
export default CreditAllocation
