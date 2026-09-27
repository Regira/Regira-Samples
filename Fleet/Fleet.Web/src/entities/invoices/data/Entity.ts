import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Supplier } from "@/entities/suppliers"
import type { Entity as Vehicle } from "@/entities/vehicles"
import type { InterventionStatus, InvoiceStatus } from "@/infrastructure/domain"

/** An intervention as listed on an invoice (API InvoiceInterventionDto - read-only, loaded on Details). */
export interface InvoiceIntervention {
    id: number
    vehicleId: number
    vehicle?: Vehicle
    status: InterventionStatus
    scheduledDate: string
    completedDate?: string
    description?: string
    totalCost: number
    lines?: Array<{ id: number; interventionTypeId: number; cost: number; interventionType?: { id: number; title?: string; code?: string } }>
}

export class Invoice extends EntityBase {
    id: number = 0
    invoiceNumber = ""
    supplierId?: number
    supplier?: Supplier // eager-loaded by the API on every row
    invoiceDate: string = new Date().toISOString().substring(0, 10) // DateOnly -> "yyyy-MM-dd"
    dueDate = ""
    status: InvoiceStatus = "Received"
    paidDate?: string
    vatRate = 21
    // derived server-side from the linked interventions (read-only here)
    subTotal = 0
    vatAmount = 0
    totalAmount = 0
    notes?: string
    // not owned: each intervention writes its own invoiceId (Details only)
    interventions?: Array<InvoiceIntervention>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.invoiceNumber || undefined
    }
}

export const Entity = Invoice
export default Invoice
