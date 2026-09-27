import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches name, e-mail and job title
    departmentId?: number
    isActive?: boolean
}

export default EntitySearchObject
