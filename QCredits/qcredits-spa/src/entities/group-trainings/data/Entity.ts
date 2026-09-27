import { EntityBase } from "@regira/modules/vue/entities"
import { GroupTrainingStatus } from "@/domain/enums"
import GroupTrainingParticipant from "../group-training-participants/Entity"

/** Company-organised group training — funded separately, never affects personal QCredit balances. */
export class GroupTraining extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    provider?: string
    location?: string
    startDate?: string // DateOnly "yyyy-MM-dd"
    durationDays = 1
    totalCost = 0
    status: GroupTrainingStatus = GroupTrainingStatus.Planned
    participantCount = 0
    participants?: Array<GroupTrainingParticipant>

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
