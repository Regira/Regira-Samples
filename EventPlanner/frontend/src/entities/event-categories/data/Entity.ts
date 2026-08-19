import { EntityBase } from "@regira/modules/vue/entities"

export class EventCategory extends EntityBase {
    id: number = 0
    title = ""
    colorHex?: string
    icon?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = EventCategory
export default EventCategory
