import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches name, VAT number, contact person and city
    interventionTypeId?: number // suppliers able to perform this type
    isActive?: boolean
    city?: string

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
