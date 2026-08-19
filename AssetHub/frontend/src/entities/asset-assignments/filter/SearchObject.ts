import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    assetId?: number
    employeeId?: number
    isActive?: boolean

    minAssignedDate?: Date
    maxAssignedDate?: Date

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
