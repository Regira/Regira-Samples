import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"
import type { EmployeeRole } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    department?: string
    role?: EmployeeRole
    isActive?: boolean

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
