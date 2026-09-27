import { EntityBase } from "@regira/modules/vue/entities"
import type { InterventionCategory } from "@/infrastructure/domain"

export class InterventionType extends EntityBase {
    id: number = 0
    title = ""
    code = ""
    description?: string
    category: InterventionCategory = "Maintenance"
    intervalKm?: number
    intervalMonths?: number
    standardCost = 0
    isActive = true

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = InterventionType
export default InterventionType
