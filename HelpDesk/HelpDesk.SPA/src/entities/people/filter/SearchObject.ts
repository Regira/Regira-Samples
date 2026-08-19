import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"
import type { PersonRole } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    role?: Array<PersonRole>
    supportTeamId?: number
    isActive?: boolean

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
