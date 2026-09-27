import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text over title, summary and author) is inherited from SearchObjectBase
    categoryId?: number // FK filter bound to an InputSelector - a scalar
    tagId?: number
    status?: string // Draft | Scheduled | Published
    isFeatured?: boolean
    author?: string
    year?: number | string
    sortBy?: string // BlogPostSortBy name - rides the query string like every other key

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
