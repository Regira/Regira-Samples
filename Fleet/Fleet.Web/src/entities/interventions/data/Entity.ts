import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Vehicle } from "@/entities/vehicles"
import type { Entity as Supplier } from "@/entities/suppliers"
import type { Entity as Invoice } from "@/entities/invoices"
import type { Entity as InterventionLine } from "../intervention-lines"
import { todayIso, type InterventionStatus, type Priority } from "@/infrastructure/domain"

export class Intervention extends EntityBase {
    id: number = 0
    vehicleId?: number
    vehicle?: Vehicle // eager-loaded by the API on every row
    supplierId?: number
    supplier?: Supplier // eager-loaded by the API on every row
    invoiceId?: number
    invoice?: Invoice // eager-loaded by the API on every row
    status: InterventionStatus = "Planned"
    priority: Priority = "Normal"
    // DateOnly on the API: "yyyy-MM-dd" strings
    scheduledDate: string = todayIso(7)
    completedDate?: string
    mileage?: number
    description?: string
    totalCost = 0 // derived server-side from the lines
    // owned rows (back-end Related()); included on lists via baseQueryParams (?includes=Lines)
    lines?: Array<InterventionLine>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        if (!this.id) return undefined
        const plate = this.vehicle?.licensePlate
        return `#${this.id}${plate ? " " + plate : ""} - ${this.scheduledDate}`
    }
}

export const Entity = Intervention
export default Intervention
