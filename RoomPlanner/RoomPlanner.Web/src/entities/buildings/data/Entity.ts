import { EntityBase } from "@regira/modules/vue/entities"

export class Building extends EntityBase {
    id: number = 0
    title = ""
    code?: string
    description?: string
    address?: string
    city?: string
    opensAt = "07:00:00" // TimeOnly — stays a string ("HH:mm:ss"), never a Date
    closesAt = "20:00:00"
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Building
export default Building
