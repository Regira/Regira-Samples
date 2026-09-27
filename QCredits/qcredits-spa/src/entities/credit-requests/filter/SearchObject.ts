import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { ActivityType, RequestStatus } from "@/domain/enums"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches title + motivation
    year?: number
    status?: RequestStatus
    activityType?: ActivityType
    employeeId?: number
    departmentId?: number
}

export default EntitySearchObject
