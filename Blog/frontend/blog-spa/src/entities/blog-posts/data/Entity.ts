import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Category } from "@/entities/categories"
import type { Entity as BlogPostTag } from "../blog-post-tags"

export class BlogPost extends EntityBase {
    id: number = 0
    title = ""
    slug = ""
    summary = ""
    content = ""
    coverImageUrl?: string
    isPublished = false
    publishedAt?: Date

    categoryId?: number
    category?: Category // populated only when the API eager-loads it — e.Includes(...), not a client includes flag

    tags?: Array<BlogPostTag>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new" // "new" (or null) marks an unsaved instance → save() inserts
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = BlogPost // the barrel name other slices import — `import type { Entity as BlogPost } from "@/entities/blog-posts"`, never `{ BlogPost }`
export default BlogPost
