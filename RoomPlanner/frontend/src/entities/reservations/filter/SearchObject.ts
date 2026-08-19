import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    organizerId?: number // filter on Employee
    roomId?: number
    buildingId?: number
    status?: string // ReservationStatus name, e.g. "Pending"
    minStartTime?: Date
    maxStartTime?: Date

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
