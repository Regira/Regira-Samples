import { EntityBase } from "@regira/modules/vue/entities"

/** The credit policy of one calendar year (defaults for new allocations). */
export class CreditYear extends EntityBase {
    id: number = 0
    year: number = new Date().getFullYear()
    annualCredits = 20
    reservedCredits = 5
    maxCarryOver = 10
    minBalance = -10
    isClosed = false
    notes?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.year ? String(this.year) : undefined
    }
}

export const Entity = CreditYear
export default CreditYear
