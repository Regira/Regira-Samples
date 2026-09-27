import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned join rows use the `_deleted` mark (never splice): removed rows are filtered out here so the
    // server deletes them by omission. No `|| []` - null = untouched, [] = delete every row.
    protected override prepareItem(item: Entity): Entity {
        item.tags = item.tags?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // IDEMPOTENT - runs inside computeds (fromPool, FormModalButton.modalTitle): every conversion is guarded.
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (typeof (e as any).publishedAt === "string") e.publishedAt = new Date((e as any).publishedAt)
        return e
    }
}

export default EntityService
