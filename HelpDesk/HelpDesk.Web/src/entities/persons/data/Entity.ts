import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as SupportTeam } from "@/entities/support-teams"

// mirrors the C# enum (serialized by name) — a const object, not a TS enum (erasableSyntaxOnly)
export const PersonRoles = { Customer: "Customer", Employee: "Employee" } as const
export type PersonRole = (typeof PersonRoles)[keyof typeof PersonRoles]

export class Person extends EntityBase {
    id: number = 0
    role: PersonRole = PersonRoles.Customer
    givenName = ""
    familyName = ""
    fullName?: string // projected by the API (read-only)
    email?: string
    phone?: string
    company?: string
    jobTitle?: string
    isActive = true
    supportTeamId?: number
    supportTeam?: SupportTeam
    hasAccount?: boolean // read-only: an Identity login is linked
    created?: Date
    lastModified?: Date

    get $isEmployee(): boolean {
        return this.role === PersonRoles.Employee
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return [this.givenName, this.familyName].filter(Boolean).join(" ") || this.fullName
    }
}

export const Entity = Person
export default Person
