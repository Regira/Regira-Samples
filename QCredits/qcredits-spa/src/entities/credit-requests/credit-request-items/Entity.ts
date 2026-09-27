import { EntityBase } from "@regira/modules/vue/entities"
import { ActivityType } from "@/domain/enums"

// An owned child row of CreditRequest (back-end `e.Related(x => x.Items)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class CreditRequestItem extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    creditRequestId?: number
    _deleted?: boolean

    type: ActivityType = ActivityType.Course
    description = ""
    provider?: string
    activityDate?: string // DateOnly "yyyy-MM-dd"
    credits = 1
    cost = 0
    url?: string
    sortOrder = 0

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.description
    }

    static create(values?: object): CreditRequestItem {
        return Object.assign(new CreditRequestItem(), values || {})
    }
}

export const Entity = CreditRequestItem
export default CreditRequestItem
