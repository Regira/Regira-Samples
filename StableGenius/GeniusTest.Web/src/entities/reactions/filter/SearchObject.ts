import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { ReactionPart, SpinStrategy } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text on the template text) is inherited from SearchObjectBase
    part?: ReactionPart
    strategy?: SpinStrategy
    isActive?: boolean
}

export default EntitySearchObject
