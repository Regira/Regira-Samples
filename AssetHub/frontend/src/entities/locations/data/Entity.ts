import { EntityBase } from "@regira/modules/vue/entities"

export class LocationItem extends EntityBase {
    id: number = 0
    title = ""
    building?: string
    room?: string
    address?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = LocationItem
export default LocationItem
