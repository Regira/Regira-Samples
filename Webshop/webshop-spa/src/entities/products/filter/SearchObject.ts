import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase
    categoryId?: number // filter on Category
    brandId?: number // filter on Brand
    minPrice?: number
    maxPrice?: number
    onSale?: boolean
    inStock?: boolean
    isFeatured?: boolean
    isActive?: boolean
    sortBy?: string // ProductSortBy name

    minCreated?: Date // `Date` is fine here — the query-string builder emits ISO-8601 with the local offset
    maxCreated?: Date
    archived?: ArchivedFilter // `only` = recycle bin, `included` = live + archived; leave unset to hide archived rows
}

export default EntitySearchObject
