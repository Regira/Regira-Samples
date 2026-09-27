import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text over title + code) is inherited
    category?: string
    isActive?: boolean

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
