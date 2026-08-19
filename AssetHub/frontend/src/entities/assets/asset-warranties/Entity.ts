import { EntityBase } from "@regira/modules/vue/entities"

// An owned child row of Asset (back-end `e.Related(x => x.Warranties)`).
export class AssetWarranty extends EntityBase {
    id: number = 0
    assetId?: number
    _deleted?: boolean

    provider = ""
    warrantyNumber?: string
    startDate?: Date
    endDate?: Date
    cost?: number
    coverageDetails?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.provider
    }

    static create(values?: object): AssetWarranty {
        const row = Object.assign(new AssetWarranty(), values || {})
        if (typeof (row as any).startDate === "string") row.startDate = new Date((row as any).startDate)
        if (typeof (row as any).endDate === "string") row.endDate = new Date((row as any).endDate)
        return row
    }
}

export const Entity = AssetWarranty
export default AssetWarranty
