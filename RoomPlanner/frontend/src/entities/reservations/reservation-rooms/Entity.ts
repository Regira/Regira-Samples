import type { Entity as MeetingRoom } from "@/entities/meeting-rooms"

// A join row linking Reservation to MeetingRoom (back-end `e.Related(x => x.Rooms)`). Rows are picked
// inside the parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark.
//
// Deliberately an interface, not an EntityBase class: InputSelectorInline constrains rows to
// `{ _deleted?: boolean; id?: number | string | null }` and nothing more, and Related() inserts rows that
// arrive without an id — so a row minted by `add(...)` needs no id, no $id and no $title. Giving a pure join
// the full model class instead is what makes `add({ meetingRoomId, meetingRoom })` fail to type-check.
//
// If this row grows scalar fields of its own (a quantity, a status, a note), it is no longer a pure join:
// re-scaffold it without --picker to get the editable-table slice instead.
export interface ReservationRoom {
    id?: number
    roomId: number
    room?: MeetingRoom // eager-loaded by the API for the chip label
    _deleted?: boolean // marked-for-removal — the parent's EntityService.prepareItem drops these before save
}

export type Entity = ReservationRoom
