import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Category } from "@/entities/categories"
import type { Entity as BlogPostTag } from "../blog-post-tags"

// mirrors the API's derived PostStatus (a const object + union, never a TS enum — erasableSyntaxOnly)
export const PostStatus = { Draft: "Draft", Scheduled: "Scheduled", Published: "Published" } as const
export type PostStatus = (typeof PostStatus)[keyof typeof PostStatus]

export class BlogPost extends EntityBase {
    id: number = 0
    title = ""
    slug?: string
    summary?: string
    content?: string
    coverImageUrl?: string
    authorName?: string
    categoryId?: number
    category?: Category // eager-loaded unconditionally by the API (e.Includes)
    isPublished = false
    publishedAt?: Date // hydrated from its ISO string in EntityService.toEntity
    isFeatured = false
    readingTimeMinutes = 0 // derived server-side from the content
    tags?: Array<BlogPostTag> // owned m2m join rows (e.Related(x => x.Tags)); loaded on Details / ?includes=Tags

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    /** Derived state: publicly visible only when published and the publication moment has passed. */
    get $status(): PostStatus {
        if (!this.isPublished) return PostStatus.Draft
        if (this.publishedAt && this.publishedAt.getTime() > Date.now()) return PostStatus.Scheduled
        return PostStatus.Published
    }
}

export const Entity = BlogPost // the barrel name other slices import — `import type { Entity as BlogPost } from "@/entities/blog-posts"`
export default BlogPost
