import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { ReservationStatus } from "@/utilities/planning"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase
    from?: Date // overlap window [from, to)
    to?: Date
    roomId?: number
    buildingId?: number
    organizerId?: number
    employeeId?: number // organizer OR attendee
    status?: Array<ReservationStatus>
    awaitingApproval?: boolean
}

export default EntitySearchObject
