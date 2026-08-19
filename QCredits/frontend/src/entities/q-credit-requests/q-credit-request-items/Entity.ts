import { EntityBase } from "@regira/modules/vue/entities"

export const CreditActivityTypes = { Course: "Course", Book: "Book", Subscription: "Subscription", SelfStudy: "SelfStudy" } as const
export type CreditActivityType = (typeof CreditActivityTypes)[keyof typeof CreditActivityTypes]

// An owned child row of QCreditRequest (back-end `e.Related(x => x.Items)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class QCreditRequestItem extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    requestId?: number // FK back to the parent (set server-side / on add)
    _deleted?: boolean // marked-for-removal — the parent's EntityService.prepareItem drops these before save

    description = ""
    type: CreditActivityType = CreditActivityTypes.Course
    credits = 1
    activityDate?: Date
    cost?: number
    provider?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.description
    }

    static create(values?: object): QCreditRequestItem {
        return Object.assign(new QCreditRequestItem(), values || {})
    }
}

export const Entity = QCreditRequestItem
export default QCreditRequestItem
