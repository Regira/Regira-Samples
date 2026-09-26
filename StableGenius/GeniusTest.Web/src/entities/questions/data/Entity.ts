import { EntityBase } from "@regira/modules/vue/entities"
import QuestionOption from "../question-options/Entity"

// C# enums mirrored as const objects (erasableSyntaxOnly rejects TS enums); the API sends names
export const QuestionType = { Choice: "Choice", Number: "Number", Rating: "Rating" } as const
export type QuestionType = (typeof QuestionType)[keyof typeof QuestionType]

export const QuestionCategory = {
    General: "General",
    Geography: "Geography",
    Math: "Math",
    Science: "Science",
    History: "History",
    Language: "Language",
    AboutYou: "AboutYou",
} as const
export type QuestionCategory = (typeof QuestionCategory)[keyof typeof QuestionCategory]

export class Question extends EntityBase {
    id: number = 0
    title = ""
    emoji?: string
    type: QuestionType = QuestionType.Choice
    category: QuestionCategory = QuestionCategory.General
    correctNumber?: number
    tolerance?: number
    minValue?: number
    maxValue?: number
    unit?: string
    revealNote?: string
    isActive = true
    alwaysAsked = false
    options?: Array<QuestionOption>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return [this.emoji, this.title].filter(Boolean).join(" ")
    }
}

export const Entity = Question
export default Question
