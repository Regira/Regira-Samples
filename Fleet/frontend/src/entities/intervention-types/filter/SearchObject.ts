import { SearchObjectBase } from "@regira/modules/vue/entities"

// Back-end InterventionTypeSearchObject adds no extra filter fields beyond the base `q` free-text search.
export class EntitySearchObject extends SearchObjectBase {}

export default EntitySearchObject
