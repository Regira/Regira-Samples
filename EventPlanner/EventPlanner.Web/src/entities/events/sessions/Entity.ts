import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Speaker } from "@/entities/speakers"

/** Owned join row Session x Speaker (nested Related() on the server) — a pure join, so a plain interface. */
export interface SessionSpeaker {
    id?: number
    sessionId?: number
    speakerId: number
    speaker?: Speaker
    _deleted?: boolean
}

// An owned child row of EventItem (back-end `e.Related(x => x.Sessions)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class Session extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    eventId?: number
    _deleted?: boolean

    title = ""
    description?: string
    startTime?: Date // UTC instant on the server, local Date here
    endTime?: Date
    room?: string
    track?: string
    capacity = 50
    speakers?: Array<SessionSpeaker>
    registeredCount?: number // computed by the API

    get $seatsLeft(): number {
        return Math.max(0, (this.capacity ?? 0) - (this.registeredCount ?? 0))
    }
    get $dayKey(): string {
        return this.startTime ? this.startTime.toLocaleDateString("sv-SE") : ""
    }
    get $timeRange(): string {
        const t = (d?: Date) => d?.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit" }) ?? ""
        return `${t(this.startTime)} - ${t(this.endTime)}`
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }

    /** Lift a plain JSON row (idempotent: dates only converted while still strings). */
    static create(values?: object): Session {
        const row = values instanceof Session ? values : Object.assign(new Session(), values || {})
        if (typeof (row as any).startTime === "string") row.startTime = new Date((row as any).startTime)
        if (typeof (row as any).endTime === "string") row.endTime = new Date((row as any).endTime)
        return row
    }
}

export const Entity = Session
export default Session
