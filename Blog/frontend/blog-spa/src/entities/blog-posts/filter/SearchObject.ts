import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase.
    categoryId?: number // FK filter bound to an InputSelector - must stay a scalar
    tagId?: number | Array<number>
    isPublished?: boolean
    minPublishedAt?: Date
    maxPublishedAt?: Date
    slug?: string

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
