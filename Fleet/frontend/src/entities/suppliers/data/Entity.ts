import { EntityBase } from "@regira/modules/vue/entities"
// barrels export the model as `Entity`, aliased here for local readability
import type { Entity as SupplierInterventionType } from "../supplier-intervention-types"

export class Supplier extends EntityBase {
    id: number = 0
    title = ""
    contactEmail?: string
    contactPhone?: string
    address?: string
    isActive = true

    supportedInterventionTypes?: Array<SupplierInterventionType>

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
