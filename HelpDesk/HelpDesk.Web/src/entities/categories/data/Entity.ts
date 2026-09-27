import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as SupportTeam } from "@/entities/support-teams"

export class Category extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    color?: string
    icon?: string
    sortOrder = 0
    isActive = true
    supportTeamId?: number
    supportTeam?: SupportTeam // routing team, eager-loaded by the API
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Category
export default Category
