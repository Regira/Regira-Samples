import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as ShoppingList } from "@/entities/shopping-lists"
import type { Entity as ArticleCategory } from "../article-categories"

export class Article extends EntityBase {
    id: number = 0
    title = ""
    notes?: string
    quantity?: number
    unit?: string
    isActive = true
    sortOrder = 0
    shoppingListId?: number
    shoppingList?: ShoppingList // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    categories?: Array<ArticleCategory> // owned join rows (back-end e.Related(x => x.Categories)) — needs ?includes=Categories

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Article // the barrel name other slices import — `import type { Entity as Article } from "@/entities/articles"`, never `{ Article }`
export default Article
