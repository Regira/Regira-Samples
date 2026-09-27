import { EntityBase } from "@regira/modules/vue/entities"

const AVATAR_COLORS = ["#7c3aed", "#ec4899", "#f97316", "#0ea5e9", "#22c55e", "#eab308", "#14b8a6", "#6366f1", "#ef4444"]

export class Speaker extends EntityBase {
    id: number = 0
    firstName = ""
    lastName = ""
    company?: string
    jobTitle?: string
    bio?: string
    email?: string
    photoUrl?: string
    topics?: string

    created?: Date
    lastModified?: Date

    get $initials(): string {
        return `${this.firstName?.[0] ?? ""}${this.lastName?.[0] ?? ""}`.toUpperCase()
    }
    get $avatarColor(): string {
        return AVATAR_COLORS[(this.id || 0) % AVATAR_COLORS.length]!
    }
    get $topics(): Array<string> {
        return (this.topics ?? "")
            .split(",")
            .map((x) => x.trim())
            .filter((x) => x)
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return [this.firstName, this.lastName].filter((x) => x).join(" ") || undefined
    }
}

export const Entity = Speaker
export default Speaker
