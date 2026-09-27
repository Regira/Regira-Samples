import { EntityBase } from "@regira/modules/vue/entities"

export const WarrantyType = { Manufacturer: "Manufacturer", Extended: "Extended", ServiceContract: "ServiceContract" } as const
export type WarrantyType = (typeof WarrantyType)[keyof typeof WarrantyType]
export const warrantyTypes = Object.values(WarrantyType)

// An owned child row of Asset (back-end `e.Related(x => x.Warranties)`), persisted with the parent's save().
export class Warranty extends EntityBase {
    id: number = 0
    assetId?: number
    _deleted?: boolean

    type: WarrantyType = WarrantyType.Manufacturer
    provider = ""
    reference?: string
    startDate = "" // DateOnly "yyyy-MM-dd"
    endDate = ""
    cost?: number
    coverage?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.provider
    }

    static create(values?: object): Warranty {
        return Object.assign(new Warranty(), values || {})
    }
}

export const Entity = Warranty
export default Warranty
