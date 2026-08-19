import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned child collections use the `_deleted` mark (never splice): removed rows are filtered out here so
    // the server deletes them by omission. Add one filter line per owned collection; `super` strips root `_`-fields.
    // ⚠️ No `|| []` — the back-end contract is null = untouched, [] = delete every row. `?.filter(...)` keeps
    // undefined for a collection this form never loaded, and still yields [] when the user removed the last row.
    protected override prepareItem(item: Entity): Entity {
        item.sessionSpeakers = item.sessionSpeakers?.filter((x) => !x._deleted)
        const prepared = super.prepareItem(item)
        delete (prepared as Partial<Entity>).seatsTaken // server-computed (SessionProcessor) — never on TInputDto
        return prepared
    }

    // Keep this IDEMPOTENT — it runs inside computeds (fromPool, FormModalButton.modalTitle). Return an
    // existing instance untouched, and guard every conversion — startTime/endTime are NOT auto-converted
    // by processItem (only created/lastModified are), so a plain fetch/pool round-trip leaves them as
    // strings, and formatDateTime()/`.getTime()` on them throws (entities.instructions → Item hydration).
    override toEntity(item: object): Entity {
        const entity = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (entity as any).startTime === "string") entity.startTime = new Date((entity as any).startTime)
        if (typeof (entity as any).endTime === "string") entity.endTime = new Date((entity as any).endTime)
        return entity
    }
}

export default EntityService
