import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches plate, make/model, supplier, type titles and description (NormalizedContent)
    vehicleId?: number // filter on Vehicle
    supplierId?: number // filter on Supplier
    invoiceId?: number // filter on Invoice
    interventionTypeId?: number
    status?: string
    priority?: string
    isInvoiced?: boolean
    minDate?: string // yyyy-MM-dd
    maxDate?: string
    sortBy?: string // InterventionSortBy (complex entity -> ?sortBy= is bound)

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
