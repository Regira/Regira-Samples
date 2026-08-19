import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Category } from "@/entities/categories"
import type { Entity as AssetStatus } from "@/entities/asset-statuses"
import type { Entity as LocationItem } from "@/entities/locations"
import type { Entity as Supplier } from "@/entities/suppliers"
import AssetAttachment from "../asset-attachments/Entity"
import AssetWarranty from "../asset-warranties/Entity"
import AssetMaintenanceRecord from "../asset-maintenance-records/Entity"
// Type-only -- erased at build time, so this doesn't form a runtime cycle with asset-assignments/data/Entity.ts
// (which type-imports Asset the same way). Only VALUE imports between slices can cycle.
import type { Entity as AssetAssignment } from "@/entities/asset-assignments"

export class Asset extends EntityBase {
    id: number = 0
    code?: string // server-generated asset tag, e.g. "AST-A1B2C3D4" -- read-only on the client
    title = ""
    description?: string
    serialNumber?: string

    categoryId?: number
    category?: Category // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    statusId?: number
    status?: AssetStatus
    locationId?: number
    location?: LocationItem
    supplierId?: number
    supplier?: Supplier

    purchaseDate?: Date
    purchasePrice?: number
    notes?: string

    isArchived?: boolean

    created?: Date
    lastModified?: Date

    attachments?: Array<AssetAttachment>
    warranties?: Array<AssetWarranty>
    maintenanceRecords?: Array<AssetMaintenanceRecord>
    // Back-reference only -- not owned via Related(), not sent on save; populated on Details via ?includes=Assignments.
    assignments?: Array<AssetAssignment>

    // Filled by the API's AssetProcessor from the currently active (unreturned) assignment -- read-only.
    currentEmployeeId?: number
    currentEmployeeName?: string
    currentAssignedDate?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Asset
export default Asset
