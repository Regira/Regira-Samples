import { EntityBase } from "@regira/modules/vue/entities"

export class Category extends EntityBase {
    id: number = 0
    title = ""
    slug = ""
    description?: string
    postCount?: number // read-only, filled server-side by CategoryProcessor

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new" // "new" (or null) marks an unsaved instance → save() inserts
    }
    override get $title(): string | undefined {
        return this.title // TODO: the human label (selectors, breadcrumbs, nav)
    }
}

export const Entity = Category // the barrel name other slices import — `import type { Entity as Category } from "@/entities/categories"`, never `{ Category }`
export default Category
