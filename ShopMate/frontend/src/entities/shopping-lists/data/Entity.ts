import { EntityBase } from "@regira/modules/vue/entities"

export class ShoppingList extends EntityBase {
    id: number = 0
    title = ""
    ownerName?: string
    description?: string
    colorHex?: string
    icon?: string
    isArchived = false
    articleCount?: number
    activeArticleCount?: number

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = ShoppingList // the barrel name other slices import — `import type { Entity as ShoppingList } from "@/entities/shopping-lists"`, never `{ ShoppingList }`
export default ShoppingList
