import { EntityBase } from "@regira/modules/vue/entities"

export class EventCategory extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    color?: string // hex, e.g. #7c3aed
    icon?: string // bootstrap-icons name without "bi-"

    created?: Date
    lastModified?: Date

    get $color(): string {
        return this.color || "#6366f1"
    }
    get $iconClass(): string {
        return `bi bi-${this.icon || "calendar-event"}`
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = EventCategory
export default EventCategory
