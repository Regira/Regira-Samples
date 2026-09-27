import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase
    buildingId?: number
    floorId?: number
    minCapacity?: number
    equipmentId?: Array<number> // AND semantics on the API: the room must offer every listed equipment
    requiresApproval?: boolean
    isActive?: boolean
    availableFrom?: Date // with availableTo: rooms free during that period
    availableTo?: Date
}

export default EntitySearchObject
