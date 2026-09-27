import { EntityBase } from "@regira/modules/vue/entities"

// Named LocationItem (not Location) so it never shadows the DOM's window.Location global
export class LocationItem extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    address?: string
    city?: string
    country?: string
    capacity = 0
    imageUrl?: string

    created?: Date
    lastModified?: Date

    get $place(): string {
        return [this.city, this.country].filter((x) => x && x !== "-").join(", ")
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = LocationItem
export default LocationItem
