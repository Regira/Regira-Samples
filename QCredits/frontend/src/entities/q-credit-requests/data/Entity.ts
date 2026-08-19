import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import type { Entity as QCreditRequestItem } from "./../q-credit-request-items"

export const RequestStatuses = { Pending: "Pending", Approved: "Approved", Rejected: "Rejected" } as const
export type RequestStatus = (typeof RequestStatuses)[keyof typeof RequestStatuses]

export class QCreditRequest extends EntityBase {
    id: number = 0
    employeeId?: number
    employee?: Employee // populated only when the API eager-loads it
    year: number = new Date().getFullYear()

    status: RequestStatus = RequestStatuses.Pending
    submittedDate?: Date
    decisionDate?: Date
    approverId?: number
    approver?: Employee
    decisionNotes?: string

    totalCredits = 0
    items?: Array<QCreditRequestItem>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee ? `${this.employee.$title} - ${this.year}` : `Request #${this.id} - ${this.year}`
    }
}

export const Entity = QCreditRequest
export default QCreditRequest
