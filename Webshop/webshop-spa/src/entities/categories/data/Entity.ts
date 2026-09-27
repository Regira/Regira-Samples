import { EntityBase } from "@regira/modules/vue/entities"

export class Category extends EntityBase {
    id: number = 0
    title = ""
    slug = ""
    description?: string
    icon?: string // Bootstrap icon name without the "bi-" prefix
    color?: string // accent colour (hex) used by the storefront
    sortOrder = 0
    productCount?: number // filled by the API's CategoryProcessor (read-only)

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    get $iconClass(): string {
        return `bi bi-${this.icon || "tag"}`
    }
}

export const Entity = Category
export default Category
