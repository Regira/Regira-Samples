import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import CreditRequestItem from "../credit-request-items/Entity"

export type WorkflowAction = "submit" | "withdraw" | "approve" | "reject" | "cancel"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // removed rows are dropped so the server deletes them by omission (null = untouched, [] = delete all)
    protected override prepareItem(item: Entity): Entity {
        item.items = item.items?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // idempotent: runs inside computeds
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (e.items?.some((row) => !(row instanceof CreditRequestItem))) e.items = e.items.map((row) => CreditRequestItem.create(row))
        return e
    }

    /** Workflow transition (POST credit-requests/{id}/{action}); returns the re-read request. */
    async transition(id: number, action: WorkflowAction, comment?: string): Promise<Entity> {
        const { data } = await this.axios.post(`${this.config.api}/${id}/${action}`, { comment })
        return this.processItem(data.item)!
    }
}

export default EntityService
