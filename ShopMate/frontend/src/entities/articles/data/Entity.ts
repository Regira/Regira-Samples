import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as ShoppingList } from "@/entities/shopping-lists"
import type { Entity as ArticleCategory } from "../article-categories"

export class Article extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    quantity?: number
    unit?: string
    isActive = true // true = still need to buy
    sortOrder = 0 // server-owned: changed through EntityService.reorder
    shoppingListId?: number
    shoppingList?: ShoppingList // loaded with ?includes=ShoppingList (and always on Details)
    categories?: Array<ArticleCategory> // loaded with ?includes=Categories (and always on Details)
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    get $amount(): string {
        return this.quantity != null ? [formatQuantity(this.quantity), this.unit].filter(Boolean).join(" ") : this.unit || ""
    }
}

function formatQuantity(value: number) {
    return Number.isInteger(value) ? String(value) : value.toLocaleString(undefined, { maximumFractionDigits: 2 })
}

export const Entity = Article
export default Article
