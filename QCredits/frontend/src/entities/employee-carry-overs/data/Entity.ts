import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"

export class EmployeeCarryOver extends EntityBase {
    id: number = 0
    employeeId?: number
    employee?: Employee // populated only when the API eager-loads it
    year: number = new Date().getFullYear()
    carriedOverCredits = 0
    note?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee ? `${this.employee.$title} - ${this.year}` : `${this.year}`
    }
}

export const Entity = EmployeeCarryOver
export default EmployeeCarryOver
