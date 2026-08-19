import { EntityBase } from "@regira/modules/vue/entities"

// An owned child row of Asset (back-end `e.Related(x => x.MaintenanceRecords)`).
export class AssetMaintenanceRecord extends EntityBase {
    id: number = 0
    assetId?: number
    _deleted?: boolean

    maintenanceDate?: Date
    performedBy = ""
    description = ""
    cost?: number
    nextDueDate?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.description
    }

    static create(values?: object): AssetMaintenanceRecord {
        const row = Object.assign(new AssetMaintenanceRecord(), values || {})
        if (typeof (row as any).maintenanceDate === "string") row.maintenanceDate = new Date((row as any).maintenanceDate)
        if (typeof (row as any).nextDueDate === "string") row.nextDueDate = new Date((row as any).nextDueDate)
        return row
    }
}

export const Entity = AssetMaintenanceRecord
export default AssetMaintenanceRecord
