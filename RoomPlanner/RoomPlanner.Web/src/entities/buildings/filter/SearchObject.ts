import { SearchObjectBase } from "@regira/modules/vue/entities"

// For<Building>() on the API: the base SearchObject only (q = keywords on title/code/city/address)
export class EntitySearchObject extends SearchObjectBase {}

export default EntitySearchObject
