import { EntityBase } from "@regira/modules/vue/entities"

export class Shopper extends EntityBase {
    id: number = 0
    name = ""
    email?: string
    color?: string
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.name
    }
    /** Two-letter avatar label. */
    get $initials(): string {
        return (this.name || "?")
            .split(/\s+/)
            .filter(Boolean)
            .slice(0, 2)
            .map((x) => x[0]!.toUpperCase())
            .join("")
    }
}

export const Entity = Shopper
export default Shopper
