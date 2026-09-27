import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (title + summary) is inherited
    categoryId?: number
    locationId?: number
    speakerId?: number
    status?: string
    isFeatured?: boolean
    upcoming?: boolean
    minDate?: string // yyyy-MM-dd (DateOnly on the server)
    maxDate?: string
}

export default EntitySearchObject
