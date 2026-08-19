import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

// Location is a bare For<Location>() on the API — only the base SearchObject<int> fields (q, dates,
// archived) are honored server-side; no custom filter fields here.
export class EntitySearchObject extends SearchObjectBase {
    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
