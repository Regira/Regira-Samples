import { EntityBase } from "@regira/modules/vue/entities"

export class GroupTraining extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    trainingDate?: Date
    location?: string
    facilitator?: string
    cost = 0
    maxParticipants = 0
    department?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = GroupTraining
export default GroupTraining
