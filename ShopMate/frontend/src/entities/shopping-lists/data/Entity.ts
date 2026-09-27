import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Shopper } from "@/entities/shoppers"

export class ShoppingList extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    color?: string = "#16a34a"
    isPinned = false
    shopperId?: number
    shopper?: Shopper // eager-loaded unconditionally by the API (e.Includes)
    // read-only counters filled by the API's ShoppingListProcessor
    articleCount?: number
    activeCount?: number
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** 0..100: share of the list that is already bought. */
    get $progress(): number {
        const total = this.articleCount ?? 0
        return total ? Math.round(((total - (this.activeCount ?? 0)) / total) * 100) : 0
    }
}

export const Entity = ShoppingList
export default ShoppingList
