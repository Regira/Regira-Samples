import { EntityBase } from "@regira/modules/vue/entities"

export class Employee extends EntityBase {
    id: number = 0
    firstName = ""
    lastName = ""
    email = ""
    department?: string
    jobTitle?: string
    isActive = true

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.firstName || this.lastName ? `${this.firstName} ${this.lastName}`.trim() : undefined
    }
}

export const Entity = Employee
export default Employee
