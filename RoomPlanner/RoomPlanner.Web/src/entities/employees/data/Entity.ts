import { EntityBase } from "@regira/modules/vue/entities"

export const departments = ["Customer Success", "Engineering", "Finance", "HR", "Legal", "Marketing", "Operations", "Product", "Sales"]

export class Employee extends EntityBase {
    id: number = 0
    firstName = ""
    lastName = ""
    title?: string // display name, maintained by the API ("First Last")
    email = ""
    department?: string
    jobTitle?: string
    phone?: string
    isActive = true
    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title || `${this.firstName} ${this.lastName}`.trim()
    }
    get $initials(): string {
        return `${this.firstName?.[0] ?? ""}${this.lastName?.[0] ?? ""}`.toUpperCase()
    }
}

export const Entity = Employee
export default Employee
