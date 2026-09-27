import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Building } from "@/entities/buildings"

export class Floor extends EntityBase {
    id: number = 0 // Guid/string-keyed API entity → `id: string = ""`; the rest of the entity slice is key-generic (owned child rows stay int)
    title = ""
    buildingId?: number
    building?: Building // populated only when the API eager-loads it — e.Includes(...), not a client includes flag
    level = 0 // storey: 0 = ground floor, negative = basement

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new" // "new" (or null) marks an unsaved instance → save() inserts
    }
    override get $title(): string | undefined {
        return this.building?.title ? `${this.building.title} · ${this.title}` : this.title
    }
}

export const Entity = Floor // the barrel name other slices import — `import type { Entity as Floor } from "@/entities/floors"`, never `{ Floor }`
export default Floor
