import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned join rows use the `_deleted` mark: drop them so Related() deletes by omission.
    // No `|| []` — null = untouched, [] = delete every row.
    protected override prepareItem(item: Entity): Entity {
        item.categories = item.categories?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    override toEntity(item: object): Entity {
        return item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
    }

    /** Tick an article off (or back on): a JSON merge-patch of the single field. */
    async setActive(id: number, isActive: boolean): Promise<Entity | undefined> {
        const { data } = await this.axios.patch(`${this.config.api}/${id}`, { isActive })
        return this.processItem(data.item)
    }

    /** Persist the order of one list: `ids` is the complete new order. */
    async reorder(shoppingListId: number, ids: Array<number>): Promise<Array<{ id: number; sortOrder: number }>> {
        const { data } = await this.axios.post(`${this.config.api}/reorder`, { shoppingListId, ids })
        return data.items
    }
}

export default EntityService
