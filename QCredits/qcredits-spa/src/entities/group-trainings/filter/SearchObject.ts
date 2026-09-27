import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { GroupTrainingStatus } from "@/domain/enums"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches title, provider and location
    year?: number
    status?: GroupTrainingStatus
    employeeId?: number
}

export default EntitySearchObject
