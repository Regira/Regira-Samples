import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as SupplierInterventionType } from "../supplier-intervention-types"

export class Supplier extends EntityBase {
    id: number = 0
    title = ""
    vatNumber?: string
    contactPerson?: string
    email?: string
    phone?: string
    street?: string
    postalCode?: string
    city?: string
    rating?: number
    isActive = true
    notes?: string
    // owned join rows (back-end Related()): the intervention types this supplier can perform
    interventionTypes?: Array<SupplierInterventionType>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Supplier
export default Supplier
