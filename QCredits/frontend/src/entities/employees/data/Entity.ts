import { EntityBase } from "@regira/modules/vue/entities"

export const EmployeeRoles = { Employee: "Employee", Admin: "Admin" } as const
export type EmployeeRole = (typeof EmployeeRoles)[keyof typeof EmployeeRoles]

export class Employee extends EntityBase {
    id: number = 0
    firstName = ""
    lastName = ""
    email = ""
    department?: string
    jobTitle?: string
    hireDate?: Date
    isActive = true
    role: EmployeeRole = EmployeeRoles.Employee

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.firstName || this.lastName ? `${this.firstName} ${this.lastName}`.trim() : this.email
    }
}

export const Entity = Employee
export default Employee
