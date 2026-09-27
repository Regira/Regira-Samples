import { EntityBase } from "@regira/modules/vue/entities"
import type { FuelType, VehicleStatus, VehicleType } from "@/infrastructure/domain"

export class Vehicle extends EntityBase {
    id: number = 0
    licensePlate = ""
    vin?: string
    make = ""
    model = ""
    year: number = new Date().getFullYear()
    vehicleType: VehicleType = "Car"
    fuelType: FuelType = "Diesel"
    status: VehicleStatus = "Active"
    mileage = 0
    department?: string
    assignedDriver?: string
    // DateOnly on the API: kept as "yyyy-MM-dd" strings (STJ reads DateOnly only in that shape)
    acquisitionDate?: string
    nextServiceDate?: string
    notes?: string

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.licensePlate ? `${this.licensePlate} - ${this.make} ${this.model}` : undefined
    }
}

export const Entity = Vehicle
export default Vehicle
