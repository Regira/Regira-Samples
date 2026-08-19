import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // sessions / sessionCount / registrationCount are server-computed / read-only (not on EventInputDto) —
    // strip them before save so the payload matches what the API actually accepts.
    protected override prepareItem(item: Entity): Entity {
        const prepared = super.prepareItem(item)
        delete (prepared as Partial<Entity>).sessions
        delete (prepared as Partial<Entity>).sessionCount
        delete (prepared as Partial<Entity>).registrationCount
        return prepared
    }

    // Keep this IDEMPOTENT — it runs inside computeds (fromPool, FormModalButton.modalTitle). Guard every
    // conversion: `event.sessions[]` arrives nested (?includes=Sessions / Details) as plain JSON with
    // string dates — lift them once here, never unconditionally (see entities.instructions → Item hydration).
    override toEntity(item: object): Entity {
        const entity = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (entity as any).startDate === "string") entity.startDate = new Date((entity as any).startDate)
        if (typeof (entity as any).endDate === "string") entity.endDate = new Date((entity as any).endDate)
        if (entity.sessions?.some((s) => typeof s.startTime === "string" || typeof s.endTime === "string")) {
            entity.sessions = entity.sessions.map((s) => ({
                ...s,
                startTime: s.startTime ? new Date(s.startTime) : undefined,
                endTime: s.endTime ? new Date(s.endTime) : undefined,
            }))
        }
        return entity
    }
}

export default EntityService
