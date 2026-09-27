import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text over title + description) is inherited from SearchObjectBase
    minCreated?: Date
    maxCreated?: Date
}

export default EntitySearchObject
