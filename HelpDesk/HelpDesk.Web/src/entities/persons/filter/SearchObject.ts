import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { PersonRole } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (name, e-mail, company) is inherited from SearchObjectBase
    role?: PersonRole
    supportTeamId?: number
    hasAccount?: boolean
    isActive?: boolean
    sortBy?: string // PersonSortBy name (Name, NameDesc, Company, Newest)
}

export default EntitySearchObject
