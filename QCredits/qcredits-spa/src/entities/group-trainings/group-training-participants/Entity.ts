import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Employee } from "@/entities/employees"

// An owned join row of GroupTraining (back-end `e.Related(x => x.Participants)`) carrying its own `attended` flag.
export class GroupTrainingParticipant extends EntityBase {
    id: number = 0
    groupTrainingId?: number
    _deleted?: boolean

    employeeId?: number
    employee?: Employee
    attended = false

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.employee ? `${this.employee.firstName} ${this.employee.lastName}` : undefined
    }

    static create(values?: object): GroupTrainingParticipant {
        return Object.assign(new GroupTrainingParticipant(), values || {})
    }
}

export const Entity = GroupTrainingParticipant
export default GroupTrainingParticipant
