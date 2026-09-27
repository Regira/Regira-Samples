import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Category } from "@/entities/categories"
import type { Entity as AssetStatus } from "@/entities/asset-statuses"
import type { Entity as Location } from "@/entities/locations"
import type { Entity as Supplier } from "@/entities/suppliers"
import type { Entity as Employee } from "@/entities/employees"
import type { Entity as EntityAttachment } from "../../entity-attachments"
import type { Entity as Warranty } from "../warranties"
import type { Entity as MaintenanceRecord } from "../maintenance-records"

/** One hand-over in the asset's history (read-only; written by the assign / return actions) */
export interface AssetAssignment {
    id: number
    assetId: number
    employeeId: number
    employee?: Employee // EmployeeRefDto (subset) - resolve through the employees pool
    assignedOn: string
    returnedOn?: string
    notes?: string
    returnNotes?: string
}

export class Asset extends EntityBase {
    id: number = 0
    code?: string // asset tag - minted by the API when left empty
    title = ""
    serialNumber?: string
    manufacturer?: string
    model?: string
    description?: string

    categoryId?: number
    category?: Category // eager-loaded unconditionally by the API
    statusId?: number
    status?: AssetStatus
    locationId?: number
    location?: Location
    supplierId?: number
    supplier?: Supplier

    purchaseDate?: string // DateOnly "yyyy-MM-dd"
    purchasePrice?: number
    orderNumber?: string

    // workflow-owned (assign / return actions); never sent back by the input DTO
    currentEmployeeId?: number
    currentEmployee?: Employee
    assignedOn?: string

    assignments?: Array<AssetAssignment>
    warranties?: Array<Warranty>
    maintenanceRecords?: Array<MaintenanceRecord>
    attachments?: Array<EntityAttachment>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.code ? `${this.code} - ${this.title}` : this.title
    }
    get $iconClass(): string {
        return `bi bi-${this.category?.icon || "box"}`
    }
}

export const Entity = Asset
export default Asset
