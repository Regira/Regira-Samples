import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import type { Entity as EventItem } from "@/entities/events"
import type { Entity as RegistrationSession } from "../registration-sessions"

export const RegistrationStatus = {
    Pending: "Pending",
    Confirmed: "Confirmed",
    Cancelled: "Cancelled",
    Attended: "Attended",
} as const
export type RegistrationStatus = (typeof RegistrationStatus)[keyof typeof RegistrationStatus]

export class Registration extends EntityBase {
    id: number = 0
    employeeId?: number
    employee?: Employee // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    eventId?: number
    event?: EventItem // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    status: RegistrationStatus = RegistrationStatus.Pending
    notes?: string
    selectedSessions?: Array<RegistrationSession>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee?.title ? `${this.employee.title} — ${this.event?.title ?? ""}` : `#${this.id}`
    }
}

export const Entity = Registration
export default Registration
