import { EntityBase } from "@regira/modules/vue/entities"

// An owned child row of Asset (back-end `e.Related(x => x.Attachments)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class AssetAttachment extends EntityBase {
    id: number = 0
    assetId?: number
    _deleted?: boolean

    fileName = ""
    contentType?: string
    sizeBytes = 0
    description?: string
    uploadedAt?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.fileName
    }

    static create(values?: object): AssetAttachment {
        const row = Object.assign(new AssetAttachment(), values || {})
        if (typeof (row as any).uploadedAt === "string") row.uploadedAt = new Date((row as any).uploadedAt)
        return row
    }
}

export const Entity = AssetAttachment
export default AssetAttachment
