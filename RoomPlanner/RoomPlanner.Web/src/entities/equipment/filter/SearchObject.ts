import { SearchObjectBase } from "@regira/modules/vue/entities"

// For<Equipment>() on the API: the base SearchObject only (q = keywords on title/code/description)
export class EntitySearchObject extends SearchObjectBase {}

export default EntitySearchObject
