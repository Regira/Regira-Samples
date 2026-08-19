import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Priority } from "@/entities/priorities"
import type { Entity as Status } from "@/entities/statuses"
import type { Entity as SupportTeam } from "@/entities/support-teams"
import type { Entity as Person } from "@/entities/people"
import type { Entity as EntityAttachment } from "../../entity-attachments"
import type { TicketCategory } from "../ticket-categories/Entity"

// Read-only conversation row - served through custom GET/POST /tickets/{id}/comments endpoints
// (see TicketService), NOT synced through Related()/the parent save path.
export interface TicketComment {
    id: number
    ticketId: number
    authorId: number
    author?: { id: number; fullName: string; email: string; role: string }
    message: string
    isInternal: boolean
    created: string
}

export class Ticket extends EntityBase {
    id: number = 0
    title = ""
    description?: string

    priorityId?: number
    priority?: Priority
    statusId?: number
    status?: Status
    supportTeamId?: number
    supportTeam?: SupportTeam
    customerId?: number
    customer?: Person
    assignedEmployeeId?: number
    assignedEmployee?: Person

    closedAt?: Date

    categories?: Array<TicketCategory>
    attachments?: Array<EntityAttachment>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = Ticket
export default Ticket
