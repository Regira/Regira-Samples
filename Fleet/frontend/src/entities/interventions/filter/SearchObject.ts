import { SearchObjectBase } from "@regira/modules/vue/entities"
import { InterventionStatus } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    vehicleId?: number
    supplierId?: number
    invoiceId?: number
    interventionTypeId?: number
    status?: Array<InterventionStatus>
    hasInvoice?: boolean
    minScheduledDate?: Date
    maxScheduledDate?: Date
}

export default EntitySearchObject
