import { EntityBase } from "@regira/modules/vue/entities"

// An owned child row of Question (back-end `e.Related(x => x.Options)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class QuestionOption extends EntityBase {
    id: number = 0
    questionId?: number
    _deleted?: boolean

    text = ""
    isCorrect = false
    customReaction?: string
    isUnreachable = false
    praiseAs?: string
    sortOrder = 0

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.text
    }

    // Lift a plain JSON row (only the root item passes through toEntity) - see EntityService.toEntity
    static create(values?: object): QuestionOption {
        return Object.assign(new QuestionOption(), values || {})
    }
}

export const Entity = QuestionOption
export default QuestionOption
