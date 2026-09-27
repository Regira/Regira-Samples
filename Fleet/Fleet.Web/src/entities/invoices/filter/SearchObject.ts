import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches the invoice number
    supplierId?: number // filter on Supplier
    status?: string
    isOverdue?: boolean
    minDate?: string // yyyy-MM-dd
    maxDate?: string

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
