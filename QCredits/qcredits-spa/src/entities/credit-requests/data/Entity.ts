import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"
import { RequestStatus } from "@/domain/enums"
import CreditRequestItem from "../credit-request-items/Entity"

/** A QCredit request: one or more purchases/activities. Only approved requests deduct credits. */
export class CreditRequest extends EntityBase {
    id: number = 0
    employeeId?: number // set server-side for employees; administrators pick one
    employee?: Employee // eager-loaded by the API on every row
    year: number = new Date().getFullYear()
    title = ""
    description?: string

    // workflow fields (read-only here; changed through the workflow actions)
    status: RequestStatus = RequestStatus.Draft
    submittedAt?: string
    decidedAt?: string
    decidedBy?: string
    decisionComment?: string

    // computed by the API
    totalCredits = 0
    totalCost = 0

    items?: Array<CreditRequestItem>

    created?: Date
    lastModified?: Date

    get isDraft(): boolean {
        return this.status === RequestStatus.Draft
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = CreditRequest
export default CreditRequest
