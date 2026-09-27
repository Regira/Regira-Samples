import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` (free text on name, company, job title, topics) is inherited
    company?: string
}

export default EntitySearchObject
