import { EntityBase } from "@regira/modules/vue/entities"

export class Status extends EntityBase {
    id: number = 0
    title = ""
    sortOrder = 0
    isClosed = false
    colorHex?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Status
export default Status
