import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { QuestionCategory, QuestionType } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text on the title) is inherited from SearchObjectBase
    type?: QuestionType
    category?: QuestionCategory
    isActive?: boolean
}

export default EntitySearchObject
