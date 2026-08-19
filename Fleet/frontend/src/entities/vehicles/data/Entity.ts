import { EntityBase } from "@regira/modules/vue/entities"

// Mirrors Fleet.Api.Entities.Vehicles.VehicleType (const object, not a TS enum -- erasableSyntaxOnly)
export const VehicleType = {
    Car: "Car",
    Van: "Van",
    Truck: "Truck",
    Trailer: "Trailer",
    Motorcycle: "Motorcycle",
} as const
export type VehicleType = (typeof VehicleType)[keyof typeof VehicleType]

// Mirrors Fleet.Api.Entities.Vehicles.VehicleStatus
export const VehicleStatus = {
    Active: "Active",
    InMaintenance: "InMaintenance",
    OutOfService: "OutOfService",
    Retired: "Retired",
} as const
export type VehicleStatus = (typeof VehicleStatus)[keyof typeof VehicleStatus]

export class Vehicle extends EntityBase {
    id: number = 0
    licensePlate = ""
    brand = ""
    model = ""
    type: VehicleType = VehicleType.Car
    status: VehicleStatus = VehicleStatus.Active
    year = new Date().getFullYear()
    mileage = 0
    vin?: string
    lastServiceDate?: Date

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.licensePlate ? `${this.licensePlate} (${this.brand} ${this.model})` : undefined
    }
}

export const Entity = Vehicle
export default Vehicle
