import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Vehicle } from "@/entities/vehicles"
import type { Entity as Supplier } from "@/entities/suppliers"
import type { Entity as Invoice } from "@/entities/invoices"
import type { Entity as InterventionInterventionType } from "../intervention-intervention-types"

// Mirrors Fleet.Api.Entities.Interventions.InterventionStatus
export const InterventionStatus = {
    Scheduled: "Scheduled",
    InProgress: "InProgress",
    Completed: "Completed",
    Cancelled: "Cancelled",
} as const
export type InterventionStatus = (typeof InterventionStatus)[keyof typeof InterventionStatus]

export class Intervention extends EntityBase {
    id: number = 0
    vehicleId?: number
    vehicle?: Vehicle
    supplierId?: number
    supplier?: Supplier
    invoiceId?: number
    invoice?: Invoice

    status: InterventionStatus = InterventionStatus.Scheduled
    scheduledDate: Date = new Date()
    completedDate?: Date
    notes?: string
    cost = 0

    interventionTypes?: Array<InterventionInterventionType>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.vehicle?.licensePlate ? `${this.vehicle.licensePlate} - ${this.status}` : `#${this.id}`
    }
}

export const Entity = Intervention
export default Intervention
