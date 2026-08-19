import { EntityBase } from "@regira/modules/vue/entities"

export class CreditPolicy extends EntityBase {
    id: number = 0
    year: number = new Date().getFullYear()
    annualCredits = 20
    reservedCredits = 5
    maxCarryOver = 10
    minBalance = -10
    freelyAvailableCredits?: number

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return `${this.year}`
    }
}

export const Entity = CreditPolicy
export default CreditPolicy
