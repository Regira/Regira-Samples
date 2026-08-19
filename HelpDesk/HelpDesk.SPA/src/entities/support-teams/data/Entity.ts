import { EntityBase } from "@regira/modules/vue/entities"

export class SupportTeam extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    memberCount?: number

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
