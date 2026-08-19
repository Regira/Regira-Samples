import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as SupportTeam } from "@/entities/support-teams"

// Mirrors the back-end PersonRole enum (by name - JsonStringEnumConverter). Const object, not a TS enum
// (erasableSyntaxOnly rejects those - see entities.setup.md -> Tooling).
export const PersonRole = { Customer: "Customer", Agent: "Agent", Admin: "Admin" } as const
export type PersonRole = (typeof PersonRole)[keyof typeof PersonRole]
export const PERSON_ROLES: Array<PersonRole> = [PersonRole.Customer, PersonRole.Agent, PersonRole.Admin]

export class Person extends EntityBase {
    id: number = 0
    fullName = ""
    email = ""
    phone?: string
    role: PersonRole = PersonRole.Customer
    company?: string
    jobTitle?: string
    isActive = true

    supportTeamId?: number
    supportTeam?: SupportTeam

    assignedTicketCount?: number

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.fullName
    }
}

export const Entity = Person
export default Person
