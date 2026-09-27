import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // The form edits the PARENT links only. Child links are written from the child's side, so they are
    // never sent (undefined → null → the API's Related() sync leaves them untouched).
    protected override prepareItem(item: Entity): Entity {
        item.parentEntities = item.parentEntities?.filter((x) => !x._deleted)
        const prepared = super.prepareItem(item)
        return Object.assign(this.createInstance(Entity as new () => Entity), prepared, { childEntities: undefined })
    }

    override toEntity(item: object): Entity {
        return item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
    }
}

export default EntityService
