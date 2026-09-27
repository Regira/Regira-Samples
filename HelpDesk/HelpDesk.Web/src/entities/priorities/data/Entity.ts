import { EntityBase } from "@regira/modules/vue/entities"

export class Priority extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    level = 1
    color?: string
    targetHours = 72
    isDefault = false
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Priority
export default Priority
