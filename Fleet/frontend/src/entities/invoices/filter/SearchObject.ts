import { SearchObjectBase } from "@regira/modules/vue/entities"
import { InvoiceStatus } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    code?: string
    supplierId?: number
    status?: Array<InvoiceStatus>
    minIssueDate?: Date
    maxIssueDate?: Date
    minDueDate?: Date
    maxDueDate?: Date
}

export default EntitySearchObject
