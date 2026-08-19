import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Asset } from "@/entities/assets"
import type { Entity as Employee } from "@/entities/employees"

export class AssetAssignment extends EntityBase {
    id: number = 0
    assetId?: number
    asset?: Asset // populated only when the API eager-loads it
    employeeId?: number
    employee?: Employee // populated only when the API eager-loads it

    assignedDate?: Date
    returnedDate?: Date
    notes?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        if (!this.employee && !this.asset) return undefined
        return `${this.employee?.$title ?? "?"} ↔ ${this.asset?.$title ?? "?"}`
    }
}

export const Entity = AssetAssignment
export default AssetAssignment
