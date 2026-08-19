import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    floorId?: number // filter on Floor
    buildingId?: number
    minCapacity?: number
    equipment?: string // one or more RoomEquipment names, e.g. "Projector, VideoConferencing"
    isActive?: boolean
    requiresApproval?: boolean

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
