import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Floor } from "@/entities/floors"

// Mirrors the back-end [Flags] enum RoomEquipment. JsonStringEnumConverter serializes a flags value as a
// comma-separated name string ("Projector, Whiteboard"), so the wire type is a plain string here - never a
// TS `enum` (erasableSyntaxOnly rejects those), and no bitmask math on the client.
export const EquipmentOptions = ["Projector", "Whiteboard", "VideoConferencing", "ConferencePhone", "Monitor", "Catering"] as const
export type EquipmentOption = (typeof EquipmentOptions)[number]

export class MeetingRoom extends EntityBase {
    id: number = 0
    title = ""
    floorId?: number
    floor?: Floor // populated only when the API eager-loads it — e.Includes(...), not a client includes flag

    capacity = 4
    equipment = "" // comma-separated RoomEquipment flag names, e.g. "Projector, Whiteboard"
    requiresApproval = false
    isActive = true

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
}

export const Entity = MeetingRoom
export default MeetingRoom
