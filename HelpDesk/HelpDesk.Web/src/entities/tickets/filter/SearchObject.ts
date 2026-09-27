import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text over code, subject and customer) is inherited from SearchObjectBase
    customerId?: number
    assignedEmployeeId?: number
    statusId?: number
    priorityId?: number
    supportTeamId?: number
    categoryId?: number
    isClosed?: boolean
    isAssigned?: boolean
    assignedToMe?: boolean
    isOverdue?: boolean
    hasAttachment?: boolean
    sortBy?: string // TicketSortBy name (Newest, Oldest, Priority, DueDate, LastActivity, Code)
    minCreated?: Date
    maxCreated?: Date
}

export default EntitySearchObject
