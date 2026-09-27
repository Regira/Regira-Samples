import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Department } from "@/entities/departments"

export class Employee extends EntityBase {
    id: number = 0
    firstName = ""
    lastName = ""
    email = ""
    jobTitle?: string
    departmentId?: number
    department?: Department // eager-loaded by the API on every row
    hireDate?: string // DateOnly: stays a "yyyy-MM-dd" string
    isActive = true

    created?: Date
    lastModified?: Date

    get fullName(): string {
        return `${this.firstName ?? ""} ${this.lastName ?? ""}`.trim()
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.fullName || this.email
    }
}

export const Entity = Employee
export default Employee
