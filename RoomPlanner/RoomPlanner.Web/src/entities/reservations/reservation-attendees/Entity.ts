import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import { AttendeeResponse } from "@/utilities/planning"

// An owned child row of Reservation (back-end `e.Related(x => x.Attendees)`): an invited employee + their response.
export class ReservationAttendee extends EntityBase {
    id: number = 0 // negative temp id for rows added in this session
    reservationId?: number
    employeeId?: number
    employee?: Employee
    response: AttendeeResponse = AttendeeResponse.NoResponse
    isOptional = false
    _deleted?: boolean

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee?.title
    }

    static create(values?: object): ReservationAttendee {
        return Object.assign(new ReservationAttendee(), values || {})
    }
}

export const Entity = ReservationAttendee
export default ReservationAttendee
