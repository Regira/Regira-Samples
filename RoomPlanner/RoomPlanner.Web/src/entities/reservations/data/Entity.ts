import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import type { Entity as ReservationRoom } from "../reservation-rooms"
import type { Entity as ReservationAttendee } from "../reservation-attendees"
import { ReservationStatus, formatRange } from "@/utilities/planning"

export class Reservation extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    organizerId?: number
    organizer?: Employee // eager-loaded unconditionally by the API
    start?: Date // UTC instant on the wire → Date (lifted in EntityService.toEntity)
    end?: Date

    // server-owned: derived by the API (prepper) or written by the workflow actions — never sent back
    status: ReservationStatus = ReservationStatus.Pending
    attendeeCount = 0
    cancelledOn?: Date
    cancelReason?: string

    rooms?: Array<ReservationRoom> // owned join rows (list rows: includes=Rooms)
    attendees?: Array<ReservationAttendee> // owned rows (Details only)
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    get $when(): string {
        return formatRange(this.start, this.end)
    }
    get $isCancelled(): boolean {
        return this.status === ReservationStatus.Cancelled
    }
    get $isPast(): boolean {
        return this.end != null && this.end.getTime() < Date.now()
    }
}

export const Entity = Reservation
export default Reservation
