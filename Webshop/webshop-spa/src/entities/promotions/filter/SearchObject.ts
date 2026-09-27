import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text) is inherited from SearchObjectBase
    live?: boolean // active + today inside its validity period

    minCreated?: Date // `Date` is fine here — the query-string builder emits ISO-8601 with the local offset
    maxCreated?: Date
    archived?: ArchivedFilter // `only` = recycle bin, `included` = live + archived; leave unset to hide archived rows
}

export default EntitySearchObject
