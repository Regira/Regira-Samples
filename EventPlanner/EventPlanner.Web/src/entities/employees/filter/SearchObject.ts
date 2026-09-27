import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` matches first/last name, e-mail and department
    department?: string
}

export default EntitySearchObject
