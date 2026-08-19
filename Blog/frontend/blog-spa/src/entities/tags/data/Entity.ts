import { EntityBase } from "@regira/modules/vue/entities"

export class Tag extends EntityBase {
    id: number = 0 // Guid/string-keyed API entity → `id: string = ""`; the rest of the entity slice is key-generic (owned child rows stay int)
    title = ""
    slug = ""

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new" // "new" (or null) marks an unsaved instance → save() inserts
    }
    override get $title(): string | undefined {
        return this.title // TODO: the human label (selectors, breadcrumbs, nav)
    }
}

export const Entity = Tag // the barrel name other slices import — `import type { Entity as Tag } from "@/entities/tags"`, never `{ Tag }`
export default Tag
