import { EntityBase } from "@regira/modules/vue/entities"

export class InterventionType extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    estimatedCost = 0
    estimatedDurationHours = 0

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
