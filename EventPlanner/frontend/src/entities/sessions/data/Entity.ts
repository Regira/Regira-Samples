import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as EventItem } from "@/entities/events"
import type { Entity as SessionSpeaker } from "../session-speakers"

export class Session extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    eventId?: number
    event?: EventItem // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    room?: string
    startTime?: Date
    endTime?: Date
    capacity = 0
    seatsTaken?: number // server-computed (SessionProcessor) — read-only, never sent on save
    sessionSpeakers?: Array<SessionSpeaker>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Session
export default Session
