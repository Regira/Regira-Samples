import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches employee number, name, e-mail and department
    locationId?: number
    department?: string
    isActive?: boolean
    hasAssets?: boolean
    sortBy?: string
}

export default EntitySearchObject
