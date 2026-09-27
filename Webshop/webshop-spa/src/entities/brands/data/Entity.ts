import { EntityBase } from "@regira/modules/vue/entities"

export class Brand extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    country?: string
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

export const Entity = Brand
export default Brand
