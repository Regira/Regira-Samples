import { SearchObjectBase } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    // `q` matches employee name/e-mail and the event title
    eventId?: number
    userId?: string
    status?: string
    upcoming?: boolean
}

export default EntitySearchObject
