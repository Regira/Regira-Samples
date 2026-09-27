import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text: title / notes) is inherited from SearchObjectBase
    shoppingListId?: number // filter on ShoppingList
    shopperId?: number
    /** The picked category first, followed by all its descendants (see useCategoryTree.expand). */
    categoryId?: number | Array<number>
    isActive?: boolean
}

export default EntitySearchObject
