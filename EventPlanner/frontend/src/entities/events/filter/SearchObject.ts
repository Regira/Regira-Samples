import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    locationId?: number // filter on Venue
    eventCategoryId?: number // filter on EventCategory
    isFeatured?: boolean
    minStartDate?: Date
    maxStartDate?: Date

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
