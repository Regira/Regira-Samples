import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text: title / description) is inherited from SearchObjectBase
    shopperId?: number // filter on Shopper
    isPinned?: boolean
    minCreated?: Date
    maxCreated?: Date
}

export default EntitySearchObject
