import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Location } from "@/entities/locations"

/** An assignment as seen from the employee (read-only; written by the asset assign / return actions) */
export interface EmployeeAssignment {
    id: number
    assetId: number
    asset?: {
        id: number
        code?: string
        title?: string
        serialNumber?: string
        category?: { id: number; title?: string; icon?: string }
        status?: { id: number; title?: string; color?: string }
    }
    employeeId: number
    assignedOn: string
    returnedOn?: string
    notes?: string
    returnNotes?: string
}

export class Employee extends EntityBase {
    id: number = 0
    code = ""
    firstName = ""
    lastName = ""
    email = ""
    phone?: string
    department?: string
    jobTitle?: string
    locationId?: number
    location?: Location // eager-loaded unconditionally by the API
    isActive = true
    hireDate?: string // DateOnly on the API: "yyyy-MM-dd" (binds to <input type="date"> as-is)
    currentAssetCount?: number // computed server-side (processor)
    assignments?: Array<EmployeeAssignment> // Details only (flag-gated include)

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        const name = `${this.firstName || ""} ${this.lastName || ""}`.trim()
        return name || this.code
    }
    get $initials(): string {
        return `${this.firstName?.[0] || ""}${this.lastName?.[0] || ""}`.toUpperCase()
    }
}

export const Entity = Employee
export default Employee
