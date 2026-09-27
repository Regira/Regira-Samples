import { EntityBase } from "@regira/modules/vue/entities"

export class Location extends EntityBase {
    id: number = 0
    title = ""
    code?: string
    address?: string
    city?: string
    country?: string
    description?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Location
export default Location
