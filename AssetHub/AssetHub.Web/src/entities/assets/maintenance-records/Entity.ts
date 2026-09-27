import { EntityBase } from "@regira/modules/vue/entities"

export const MaintenanceType = { Preventive: "Preventive", Repair: "Repair", Inspection: "Inspection", Upgrade: "Upgrade", Cleaning: "Cleaning" } as const
export type MaintenanceType = (typeof MaintenanceType)[keyof typeof MaintenanceType]
export const maintenanceTypes = Object.values(MaintenanceType)

// An owned child row of Asset (back-end `e.Related(x => x.MaintenanceRecords)`), persisted with the parent's save().
export class MaintenanceRecord extends EntityBase {
    id: number = 0
    assetId?: number
    _deleted?: boolean

    type: MaintenanceType = MaintenanceType.Preventive
    date = "" // DateOnly "yyyy-MM-dd"
    title = ""
    description?: string
    performedBy?: string
    cost?: number
    nextDueDate?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }

    static create(values?: object): MaintenanceRecord {
        return Object.assign(new MaintenanceRecord(), values || {})
    }
}

export const Entity = MaintenanceRecord
export default MaintenanceRecord
