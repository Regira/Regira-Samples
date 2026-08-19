import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase.
    isRoot?: boolean // true = only top-level categories
}

export default EntitySearchObject
