import { EntityBase } from "@regira/modules/vue/entities"

export class Equipment extends EntityBase {
    id: number = 0
    title = ""
    code?: string
    description?: string
    icon?: string // bootstrap-icons name without the "bi-" prefix
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** A usable icon class (falls back to a generic tool icon). */
    get $iconClass(): string {
        return `bi bi-${this.icon || "tools"}`
    }
}

export const Entity = Equipment
export default Equipment
