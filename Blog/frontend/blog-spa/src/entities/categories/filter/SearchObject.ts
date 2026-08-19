import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text keyword search) is inherited from SearchObjectBase - matches everything the
    // API's CategorySearchObject supports (no extra filter fields on the back-end).
    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
