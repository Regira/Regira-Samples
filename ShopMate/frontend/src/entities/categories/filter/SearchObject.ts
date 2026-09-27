import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text: title / description) is inherited from SearchObjectBase
    parentId?: number
    childId?: number
    isRoot?: boolean
}

export default EntitySearchObject
