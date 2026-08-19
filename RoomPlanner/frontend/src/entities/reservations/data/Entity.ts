import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import type { ReservationRoom } from "../reservation-rooms/Entity"
import { ReservationAttendee } from "../reservation-attendees/Entity"

// Mirrors the back-end enum ReservationStatus - a const object + union type, never a TS `enum`
// (erasableSyntaxOnly rejects those; JsonStringEnumConverter serializes it as a name string anyway).
export const ReservationStatuses = ["Pending", "Approved", "Rejected", "Cancelled"] as const
export type ReservationStatus = (typeof ReservationStatuses)[number]

export class Reservation extends EntityBase {
    id: number = 0
    title = "" // meeting subject
    description?: string
    startTime: Date = new Date()
    endTime: Date = new Date()

    organizerId?: number
    organizer?: Employee // populated only when the API eager-loads it — e.Includes(...), not a client includes flag

    status: ReservationStatus = "Pending"

    rooms?: Array<ReservationRoom>
    attendees?: Array<ReservationAttendee>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Reservation
export default Reservation
