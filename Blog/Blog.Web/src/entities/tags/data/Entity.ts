import { EntityBase } from "@regira/modules/vue/entities"

export class Tag extends EntityBase {
    id: number = 0
    title = ""
    slug?: string
    // [NotMapped] counts filled by the API's TagProcessor (null on nested rows)
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

export const Entity = Tag // the barrel name other slices import — `import type { Entity as Tag } from "@/entities/tags"`
export default Tag
