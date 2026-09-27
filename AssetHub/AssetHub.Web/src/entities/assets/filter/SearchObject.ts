import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { StatusKind } from "@/entities/asset-statuses/data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches asset tag, name, serial number, manufacturer and model
    categoryId?: number
    statusId?: number
    statusKind?: StatusKind
    locationId?: number
    supplierId?: number
    employeeId?: number // current holder
    isAssigned?: boolean
    hasAttachment?: boolean
    underWarranty?: boolean
    warrantyExpiresBefore?: string // "yyyy-MM-dd"
    maintenanceDueBefore?: string // "yyyy-MM-dd"
    sortBy?: string // AssetSortBy member name
}

export default EntitySearchObject
