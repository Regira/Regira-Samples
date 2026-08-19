import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"

export const ResponseStatuses = ["Invited", "Accepted", "Declined", "Tentative"] as const
export type ResponseStatus = (typeof ResponseStatuses)[number]

// An owned child row of Reservation (back-end `e.Related(x => x.Attendees)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
// Carries both a relation (an internal Employee, optional) and scalars (external guest info, response) -
// rendered as a table with an InputSelector in the relation column.
export class ReservationAttendee extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    reservationId?: number // FK back to the parent (set server-side / on add)
    _deleted?: boolean // marked-for-removal — the parent's EntityService.prepareItem drops these before save

    employeeId?: number
    employee?: Employee // eager-loaded by the API for the pooled label

    externalName?: string
    externalEmail?: string

    responseStatus: ResponseStatus = "Invited"

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee?.title ?? this.externalName
    }

    /**
     * Named constructor for a row that arrives as plain JSON — every stored row does, since only the ROOT
     * item passes through a service's `toEntity`. Used in the owning service's `toEntity` to lift the
     * collection (guarded there to stay idempotent) and to seed the add-row with real defaults/getters.
     */
    static create(values?: object): ReservationAttendee {
        return Object.assign(new ReservationAttendee(), values || {})
    }
}

export const Entity = ReservationAttendee
export default ReservationAttendee
