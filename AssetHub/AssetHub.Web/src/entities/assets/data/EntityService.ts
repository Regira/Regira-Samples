import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import { Warranty } from "../warranties/Entity"
import { MaintenanceRecord } from "../maintenance-records/Entity"
import { insertWithAttachments, updateWithAttachments } from "../../entity-attachments/data/functions"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // ---- domain actions (AssetWorkflowController) - answer with a re-read: { item } ----
    async assign(id: number, employeeId: number, notes?: string): Promise<Entity> {
        const { data } = await this.axios.post(`${this.config.api}/${id}/assign`, { employeeId, notes })
        return this.processItem(data.item)!
    }
    async returnAsset(id: number, notes?: string, statusId?: number): Promise<Entity> {
        const { data } = await this.axios.post(`${this.config.api}/${id}/return`, { notes, statusId })
        return this.processItem(data.item)!
    }

    override async insert(item: Entity): Promise<Entity | undefined> {
        return await insertWithAttachments(this.config.api, item, () => super.insert(item), (saved) => super.update(saved))
    }
    override async update(item: Entity): Promise<Entity | undefined> {
        return await updateWithAttachments(this.config.api, item, () => super.update(item))
    }

    // Owned collections: drop rows marked _deleted so Related() deletes them by omission (null = untouched).
    protected override prepareItem(item: Entity): Entity {
        item.warranties = item.warranties?.filter((x) => !x._deleted)
        item.maintenanceRecords = item.maintenanceRecords?.filter((x) => !x._deleted)
        item.attachments = item.attachments?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // Idempotent: returns an existing instance untouched and lifts owned rows only when they are still plain JSON.
    override toEntity(item: object): Entity {
        const e = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        if (e.warranties?.some((row) => !(row instanceof Warranty))) e.warranties = e.warranties.map((row) => Warranty.create(row))
        if (e.maintenanceRecords?.some((row) => !(row instanceof MaintenanceRecord)))
            e.maintenanceRecords = e.maintenanceRecords.map((row) => MaintenanceRecord.create(row))
        return e
    }
}

export default EntityService
