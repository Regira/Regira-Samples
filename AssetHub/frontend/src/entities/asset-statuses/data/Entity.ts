import { EntityBase } from "@regira/modules/vue/entities"

export class AssetStatus extends EntityBase {
    id: number = 0
    title = ""
    colorHex = "#64748b"
    isOperational = true
    sortOrder = 0

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = AssetStatus
export default AssetStatus
