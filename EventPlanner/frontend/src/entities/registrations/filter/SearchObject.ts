import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"
import { RegistrationStatus } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    employeeId?: number // filter on Employee
    eventId?: number // filter on EventItem
    status?: RegistrationStatus

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
