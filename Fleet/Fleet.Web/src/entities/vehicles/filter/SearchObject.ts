import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches plate, VIN, make, model, department and driver (NormalizedContent)
    vehicleType?: string
    fuelType?: string
    status?: string
    department?: string
    serviceDueBefore?: string // yyyy-MM-dd
    minYear?: number
    maxYear?: number
    sortBy?: string // VehicleSortBy (complex entity -> ?sortBy= is bound)

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
