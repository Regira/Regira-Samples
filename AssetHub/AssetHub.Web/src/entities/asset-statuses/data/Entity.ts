import { EntityBase } from "@regira/modules/vue/entities"

// mirrors the C# StatusKind enum as a const object + union (erasableSyntaxOnly)
export const StatusKind = { Available: "Available", Assigned: "Assigned", Maintenance: "Maintenance", Inactive: "Inactive" } as const
export type StatusKind = (typeof StatusKind)[keyof typeof StatusKind]
export const statusKinds = Object.values(StatusKind)

export class AssetStatus extends EntityBase {
    id: number = 0
    title = ""
    description?: string
    color = "#6c757d"
    kind: StatusKind = StatusKind.Available
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
