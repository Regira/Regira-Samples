import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Floor } from "@/entities/floors"
import type { Entity as RoomEquipment } from "../room-equipments"

export class Room extends EntityBase {
    id: number = 0
    title = ""
    code?: string
    description?: string
    floorId?: number
    floor?: Floor // eager-loaded unconditionally by the API (floor + building on every row)
    capacity = 4
    requiresApproval = false
    isActive = true
    color?: string
    equipment?: Array<RoomEquipment> // owned join rows (Room.Related(x => x.Equipment)); list rows get them via includes=Equipment
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** "Atrium Tower · Floor 2" */
    get $location(): string {
        const building = this.floor?.building?.title
        return [building, this.floor?.title].filter(Boolean).join(" · ")
    }
    get $color(): string {
        return this.color || "#6c757d"
    }
}

export const Entity = Room
export default Room
