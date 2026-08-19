import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"
import type { RequestStatus } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    employeeId?: number
    approverId?: number
    year?: number
    status?: RequestStatus

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
