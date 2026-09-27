import { EntityBase } from "@regira/modules/vue/entities"

export class Category extends EntityBase {
    id: number = 0
    title = ""
    slug?: string
    description?: string
    color?: string
    sortOrder = 0
    // [NotMapped] counts filled by the API's CategoryProcessor (null on nested rows)
    postCount?: number
    publishedPostCount?: number

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Category // the barrel name other slices import — `import type { Entity as Category } from "@/entities/categories"`
export default Category
