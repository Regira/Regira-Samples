import { EntityBase } from "@regira/modules/vue/entities"

/** Minimal projection of a category, as nested in link rows and article chips. */
export interface CategoryCore {
    id: number
    title: string
    icon?: string
    color?: string
}
/** Link row "parent contains child" (back-end RelatedCategory, owned via e.Related()). */
export interface RelatedCategory {
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
    description?: string
    icon?: string
    color?: string = "#6c757d"
    parentEntities?: Array<RelatedCategory>
    childEntities?: Array<RelatedCategory>
    articleCount?: number // filled by the API's CategoryProcessor
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    get $label(): string {
        return [this.icon, this.title].filter(Boolean).join(" ")
    }
    get $isRoot(): boolean {
        return !this.parentEntities?.length
    }
}

export const Entity = Category
export default Category
