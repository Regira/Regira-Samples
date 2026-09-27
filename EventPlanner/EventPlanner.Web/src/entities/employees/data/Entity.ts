import { EntityBase } from "@regira/modules/vue/entities"

// Read-only projection of an ASP.NET Identity user (GET /api/employees) — not a Regira entity on the server
export class Employee extends EntityBase {
    id: string = ""
    firstName?: string
    lastName?: string
    email?: string
    department?: string
    jobTitle?: string

    get $initials(): string {
        return `${this.firstName?.[0] ?? ""}${this.lastName?.[0] ?? ""}`.toUpperCase() || "?"
    }

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return [this.firstName, this.lastName].filter((x) => x).join(" ") || this.email
    }
}

export const Entity = Employee
export default Employee
