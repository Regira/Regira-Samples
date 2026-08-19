import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Venue } from "@/entities/locations"
import type { Entity as EventCategory } from "@/entities/event-categories"

// Lightweight nested shape — mirrors the API's SessionCoreDto (Event.Sessions), not the full Session
// model. Details always eager-loads it; read-only here (never sent back on save).
export interface SessionCore {
    id: number
    title: string
    room?: string
    startTime?: Date
    endTime?: Date
    capacity: number
    seatsTaken?: number
}

export class EventItem extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    bannerImageUrl?: string
    locationId?: number
    location?: Venue // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    eventCategoryId?: number
    eventCategory?: EventCategory // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    startDate?: Date
    endDate?: Date
    isFeatured = false
    sessionCount?: number // server-computed (EventProcessor) — read-only
    registrationCount?: number // server-computed (EventProcessor) — read-only
    sessions?: Array<SessionCore> // eager-loaded on Details (always — Details applies every registered include)

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = EventItem
export default EventItem
