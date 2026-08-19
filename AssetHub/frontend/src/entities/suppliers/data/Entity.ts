import { EntityBase } from "@regira/modules/vue/entities"

export class Supplier extends EntityBase {
    id: number = 0
    title = ""
    contactName?: string
    email?: string
    phone?: string
    website?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Supplier
export default Supplier
