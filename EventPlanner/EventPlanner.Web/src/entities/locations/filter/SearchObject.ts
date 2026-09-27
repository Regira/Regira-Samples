import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free text on title, city, address, country) is inherited
    city?: string
    minCapacity?: number
}

export default EntitySearchObject
