import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

// Speaker is a bare For<Speaker>() on the API — only the base SearchObject<int> fields are honored
// server-side (q covers name/company/jobTitle via NormalizedContent); no custom filter fields here.
export class EntitySearchObject extends SearchObjectBase {
    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
