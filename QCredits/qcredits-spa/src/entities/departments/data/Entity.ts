import { EntityBase } from "@regira/modules/vue/entities"

export class Department extends EntityBase {
    id: number = 0
    title = ""
    code?: string
    description?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Department
export default Department
