import { EntityBase } from "@regira/modules/vue/entities"

export class Speaker extends EntityBase {
    id: number = 0
    title = "" // full name
    description?: string // bio
    jobTitle?: string
    company?: string
    email?: string
    photoUrl?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Speaker
export default Speaker
