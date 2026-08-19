import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    categoryId?: number
    statusId?: number
    locationId?: number
    supplierId?: number
    isAssigned?: boolean
    assignedToEmployeeId?: number

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
