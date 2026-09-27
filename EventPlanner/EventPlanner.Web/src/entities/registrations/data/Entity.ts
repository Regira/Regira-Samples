import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as EventItem } from "@/entities/events"
import type { Entity as Employee } from "@/entities/employees"

// mirrors the C# enum RegistrationStatus (serialized by name)
export const RegistrationStatus = { Confirmed: "Confirmed", Waitlisted: "Waitlisted", Cancelled: "Cancelled" } as const
export type RegistrationStatus = (typeof RegistrationStatus)[keyof typeof RegistrationStatus]

/** Owned join row Registration x Session (the sessions the employee picked). */
export interface RegistrationSession {
    id?: number
    registrationId?: number
    sessionId: number
    session?: { id: number; title?: string; startTime?: string; endTime?: string; room?: string }
    _deleted?: boolean
}

export class Registration extends EntityBase {
    id: number = 0
    eventId?: number
    event?: EventItem // eager-loaded summary (title, dates, status)
    userId?: string // set by the server for employees; chosen by administrators on create
    user?: Employee // eager-loaded
    status: RegistrationStatus = RegistrationStatus.Confirmed
    notes?: string
    sessions?: Array<RegistrationSession>

    created?: Date
    lastModified?: Date

    get $isActive(): boolean {
        return this.status !== RegistrationStatus.Cancelled
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        const who = [this.user?.firstName, this.user?.lastName].filter((x) => x).join(" ")
        return [this.event?.title, who].filter((x) => x).join(" - ") || undefined
    }
}

export const Entity = Registration
export default Registration
