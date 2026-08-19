import { EntityBase } from "@regira/modules/vue/entities"

// A self-referencing many-to-many hierarchy: RelatedCategoryRef rows carry BOTH ends of the join
// (parentId/childId) plus the resolved sibling's core fields — a category can list several parents
// and several children at once. Hand-modeled rather than scaffolded as an owned sub-slice: a
// self-relation is wired by hand (scaffold.mjs skips --rel naming the entity being scaffolded).
export interface CategoryCore {
    id: number
    title: string
    icon?: string
    colorHex?: string
}
export interface RelatedCategoryRef {
    id?: number
    parentId: number
    childId: number
    parent?: CategoryCore
    child?: CategoryCore
    _deleted?: boolean
}

export class Category extends EntityBase {
    id: number = 0
    title = ""
    icon?: string
    colorHex?: string
    articleCount?: number
    parentEntities?: Array<RelatedCategoryRef>
    childEntities?: Array<RelatedCategoryRef>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Category // the barrel name other slices import — `import type { Entity as Category } from "@/entities/categories"`, never `{ Category }`
export default Category
