import { SearchObjectBase } from "@regira/modules/vue/entities"
import type { StatusKind } from "../data/Entity"

export class EntitySearchObject extends SearchObjectBase {
    kind?: StatusKind
}

export default EntitySearchObject
