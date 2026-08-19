import { SearchObjectBase } from "@regira/modules/vue/entities"
import { VehicleType, VehicleStatus } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    type?: Array<VehicleType>
    status?: Array<VehicleStatus>
    minYear?: number
    maxYear?: number
}

export default EntitySearchObject
