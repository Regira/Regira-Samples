import type { AxiosInstance } from "axios"
import { EntityServiceBase, type IConfig } from "@regira/modules/vue/entities"
import Entity from "./Entity"
import AssetAttachment from "../asset-attachments/Entity"
import AssetWarranty from "../asset-warranties/Entity"
import AssetMaintenanceRecord from "../asset-maintenance-records/Entity"

export class EntityService extends EntityServiceBase<Entity> {
    constructor(axios: AxiosInstance, config: IConfig) {
        super(axios, config)
    }

    // Owned child collections use the `_deleted` mark (never splice): removed rows are filtered out here so
    // the server deletes them by omission. Add one filter line per owned collection; `super` strips root `_`-fields.
    // ⚠️ No `|| []` — the back-end contract is null = untouched, [] = delete every row. `?.filter(...)` keeps
    // undefined for a collection this form never loaded, and still yields [] when the user removed the last row.
    protected override prepareItem(item: Entity): Entity {
        item.attachments = item.attachments?.filter((x) => !x._deleted)
        item.warranties = item.warranties?.filter((x) => !x._deleted)
        item.maintenanceRecords = item.maintenanceRecords?.filter((x) => !x._deleted)
        return super.prepareItem(item)
    }

    // Keep this IDEMPOTENT — it runs inside computeds (fromPool, FormModalButton.modalTitle). Return an
    // existing instance untouched, and guard every conversion you add — dates:
    //   if (typeof (e as any).publishedOn === "string") e.publishedOn = new Date((e as any).publishedOn)
    // and owned-collection lifts, where a fresh array is a mutation just like a fresh Date:
    //   if (e.children?.some((row) => !(row instanceof Child))) e.children = e.children.map((row) => Child.create(row))
    // An unconditional conversion throws "Maximum recursive updates exceeded" against a LIBRARY component.
    override toEntity(item: object): Entity {
        const entity = item instanceof Entity ? item : Object.assign(this.createInstance(Entity as new () => Entity), item || {})
        // created/lastModified are hydrated automatically by processItem(); every other date field is not.
        if (typeof (entity as any).purchaseDate === "string") entity.purchaseDate = new Date((entity as any).purchaseDate)
        if (typeof (entity as any).currentAssignedDate === "string") entity.currentAssignedDate = new Date((entity as any).currentAssignedDate)
        // useOwnedCollection hands raw rows to the view -- only the root item passes through toEntity, so the
        // owned collections are lifted here (idempotent: only maps when a row isn't already the model class).
        if (entity.attachments?.some((row) => !(row instanceof AssetAttachment))) {
            entity.attachments = entity.attachments.map((row) => AssetAttachment.create(row))
        }
        if (entity.warranties?.some((row) => !(row instanceof AssetWarranty))) {
            entity.warranties = entity.warranties.map((row) => AssetWarranty.create(row))
        }
        if (entity.maintenanceRecords?.some((row) => !(row instanceof AssetMaintenanceRecord))) {
            entity.maintenanceRecords = entity.maintenanceRecords.map((row) => AssetMaintenanceRecord.create(row))
        }
        return entity
    }
}

export default EntityService
