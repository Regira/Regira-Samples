import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import QCreditRequestItem from "../q-credit-request-items/Entity"

export interface DecisionInput {
    approverId: number
    notes?: string
}
export interface DecisionResult {
    id: number
    status: string
    decisionDate?: string
    approverId?: number
    decisionNotes?: string
}

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned collection: drop rows marked _deleted so Related() deletes them by omission.
    protected override prepareItem(item: Entity): Entity {
        item.items = item.items?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // Lift the nested Items rows into QCreditRequestItem instances (only the root passes through toEntity).
    // Guarded with .some(...) so this stays idempotent - toEntity runs inside computeds (fromPool, etc).
    override toEntity(item: object): Entity {
        const entity = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (entity.items?.some((row) => !(row instanceof QCreditRequestItem))) {
            entity.items = entity.items.map((row) => QCreditRequestItem.create(row))
        }
        return entity
    }

    // Domain actions - not part of the CRUD save path. See Controllers/QCreditRequestWorkflowController.cs.
    async approve(id: number, input: DecisionInput): Promise<DecisionResult> {
        const { data } = await this.axios.post<DecisionResult>(`${this.config.api}/${id}/approve`, input)
        return data
    }
    async reject(id: number, input: DecisionInput): Promise<DecisionResult> {
        const { data } = await this.axios.post<DecisionResult>(`${this.config.api}/${id}/reject`, input)
        return data
    }
}

export default EntityService
