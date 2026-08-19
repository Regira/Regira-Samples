import { EntityBase } from "@regira/modules/vue/entities"

export class Building extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    address = ""
    city = ""

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
