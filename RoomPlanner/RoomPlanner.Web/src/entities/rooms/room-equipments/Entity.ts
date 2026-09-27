import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Equipment } from "@/entities/equipment"

// An owned child row of Room (back-end `e.Related(x => x.Equipment)`): one equipment type + a quantity.
// Rows are edited inside the parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark.
export class RoomEquipment extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    roomId?: number
    equipmentId?: number
    equipment?: Equipment
    quantity = 1
    _deleted?: boolean

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.equipment?.title
    }

    static create(values?: object): RoomEquipment {
        return Object.assign(new RoomEquipment(), values || {})
    }
}

export const Entity = RoomEquipment
export default RoomEquipment
