import { SearchObjectBase, ArchivedFilter } from "@regira/modules/vue/entities"

export class EntitySearchObject extends SearchObjectBase {
    eventId?: number // filter on EventItem
    speakerId?: number // filter on Speaker
    minStartTime?: Date
    maxStartTime?: Date

    minCreated?: Date
    maxCreated?: Date
    archived?: ArchivedFilter
}

export default EntitySearchObject
