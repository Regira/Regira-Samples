import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` searches name, contact and e-mail (normalized server-side)
    minCreated?: Date
    maxCreated?: Date
}

export default EntitySearchObject
