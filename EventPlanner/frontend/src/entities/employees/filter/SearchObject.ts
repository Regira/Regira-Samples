import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

// Employee is a bare For<Employee>() on the API — only the base SearchObject<int> fields are honored
// server-side (q covers name/email/department via NormalizedContent); no custom filter fields here.
export class EntitySearchObject extends SearchObjectBase {
    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
