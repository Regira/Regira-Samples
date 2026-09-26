import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { GameSortBy } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free-text on player name + title) is inherited from SearchObjectBase
    isFinished?: boolean
    sortBy?: GameSortBy
}

export default EntitySearchObject
