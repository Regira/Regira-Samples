import type { Entity as Room } from "@/entities/rooms"
import type { RoomApprovalStatus } from "@/utilities/planning"

// A join row linking Reservation to Room (back-end `e.Related(x => x.Rooms)`). From the client's point of view it
// is a pure join (only roomId is sent); the approval fields are server-owned and read-only here — they change
// through the workflow endpoints (approve / reject) only.
export interface ReservationRoom {
    id?: number
    roomId: number
    room?: Room // eager-loaded by the API for the chip label
    approvalStatus?: RoomApprovalStatus
    decidedOn?: string
    decisionNote?: string
    _deleted?: boolean // marked-for-removal — the parent's EntityService.prepareItem drops these before save
}

export type Entity = ReservationRoom
