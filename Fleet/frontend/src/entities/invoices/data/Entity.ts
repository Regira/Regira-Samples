import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Supplier } from "@/entities/suppliers"

// Mirrors Fleet.Api.Entities.Invoices.InvoiceStatus
export const InvoiceStatus = {
    Draft: "Draft",
    Sent: "Sent",
    Paid: "Paid",
    Overdue: "Overdue",
    Cancelled: "Cancelled",
} as const
export type InvoiceStatus = (typeof InvoiceStatus)[keyof typeof InvoiceStatus]

// Read-only projection of the interventions billed on this invoice (Details eager-loads them
// unconditionally -- Invoice does NOT own this collection via e.Related(), so it is never sent back
// on save; each Intervention's own service is the writer of its InvoiceId).
export interface InvoiceInterventionSummary {
    id: number
    vehicleId: number
    vehicle?: { licensePlate: string; brand: string; model: string }
    status: string
    scheduledDate: string | Date
    cost: number
}

export class Invoice extends EntityBase {
    id: number = 0
    code?: string // server-generated (INV-{year}-{00001}) -- read-only on the client
    supplierId?: number
    supplier?: Supplier

    status: InvoiceStatus = InvoiceStatus.Draft
    issueDate: Date = new Date()
    dueDate: Date = new Date()
    totalAmount = 0 // server-owned aggregate -- read-only on the client
    interventions?: Array<InvoiceInterventionSummary>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.code ?? `#${this.id}`
    }
}

export const Entity = Invoice
export default Invoice
