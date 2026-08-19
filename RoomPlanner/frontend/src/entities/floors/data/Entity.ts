import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Building } from "@/entities/buildings"

export class Floor extends EntityBase {
    id: number = 0
    title = ""
    level = 0
    buildingId?: number
    building?: Building // populated only when the API eager-loads it — e.Includes(...), not a client includes flag

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new" // "new" (or null) marks an unsaved instance → save() inserts
    }
    override get $title(): string | undefined {
        return this.title // TODO: the human label (selectors, breadcrumbs, nav)
    }
}

export const Entity = Floor // the barrel name other slices import — `import type { Entity as Floor } from "@/entities/floors"`, never `{ Floor }`
export default Floor
