import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase.
    shoppingListId?: number // filter on ShoppingList
    categoryId?: number // populated from code (chip row), not from an InputSelector — see FilterAdv.vue
    isActive?: boolean // true = still need to buy, false = already bought, undefined = both
}

export default EntitySearchObject
