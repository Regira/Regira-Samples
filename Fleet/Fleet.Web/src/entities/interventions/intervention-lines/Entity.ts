import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as InterventionType } from "@/entities/intervention-types"

// An owned child row of Intervention (back-end `e.Related(x => x.Lines)`): one intervention type performed,
// with its cost. Edited inside the parent form, persisted with the parent's single save(); removal = `_deleted`.
export class InterventionLine extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    interventionId?: number
    _deleted?: boolean

    interventionTypeId?: number
    interventionType?: InterventionType // eager-loaded by the API (label + standard cost)
    cost = 0
    remarks?: string

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.interventionType?.title
    }

    /** lift a plain JSON row (only the root item passes through toEntity) */
    static create(values?: object): InterventionLine {
        return Object.assign(new InterventionLine(), values || {})
    }
}

export const Entity = InterventionLine
export default InterventionLine
