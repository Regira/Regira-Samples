import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Person } from "@/entities/persons"

export class SupportTeam extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    email?: string
    color?: string
    isActive = true
    members?: Array<Person> // Details only (the API gates the collection)
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = SupportTeam
export default SupportTeam
