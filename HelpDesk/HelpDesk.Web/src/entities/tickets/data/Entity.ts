import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Person } from "@/entities/persons"
import type { Entity as Status } from "@/entities/statuses"
import type { Entity as Priority } from "@/entities/priorities"
import type { Entity as SupportTeam } from "@/entities/support-teams"
import type { Entity as EntityAttachment } from "../../entity-attachments"
import type { Entity as TicketCategory } from "../ticket-categories"

/** One message of the conversation thread (written only through POST tickets/{id}/comments). */
export interface TicketComment {
    id: number
    ticketId: number
    authorId: number
    author?: Person
    body: string
    isInternal: boolean
    created?: Date
}

export class Ticket extends EntityBase {
    id: number = 0
    code?: string // server-minted (HD-000123)
    title = "" // the subject
    description?: string
    customerId?: number
    customer?: Person
    assignedEmployeeId?: number
    assignedEmployee?: Person
    statusId?: number
    status?: Status
    priorityId?: number
    priority?: Priority
    supportTeamId?: number
    supportTeam?: SupportTeam
    dueDate?: Date
    closedAt?: Date
    firstResponseAt?: Date
    categories?: Array<TicketCategory>
    comments?: Array<TicketComment> // read-only on the ticket: Details (or ?includes=Comments) only
    attachments?: Array<EntityAttachment>
    hasAttachment?: boolean
    commentCount?: number
    attachmentCount?: number

    created?: Date
    lastModified?: Date

    get $isClosed(): boolean {
        return !!this.status?.isClosed
    }
    get $isOverdue(): boolean {
        return !this.$isClosed && this.dueDate != null && new Date(this.dueDate).getTime() < Date.now()
    }
    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.code ? `${this.code} · ${this.title}` : this.title
    }
}

export const Entity = Ticket
export default Ticket
