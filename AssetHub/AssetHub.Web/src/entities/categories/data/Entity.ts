import { EntityBase } from "@regira/modules/vue/entities"

export class Category extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    icon?: string
    lifespanMonths?: number

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** Bootstrap-Icons class for the category icon hint */
    get $iconClass(): string {
        return `bi bi-${this.icon || "box"}`
    }
}

export const Entity = Category
export default Category
