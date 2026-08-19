import { EntityBase } from "@regira/modules/vue/entities"

export class Venue extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    address = ""
    city = ""
    postalCode?: string
    country = ""
    capacity = 0
    imageUrl?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Venue
export default Venue
