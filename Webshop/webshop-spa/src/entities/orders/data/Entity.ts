import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as OrderLine } from "../order-lines"

// mirrors of the C# enums (serialized as names) — const objects + union types, no TS enums
export const OrderStatuses = ["Pending", "Paid", "Processing", "Shipped", "Delivered", "Cancelled"] as const
export type OrderStatus = (typeof OrderStatuses)[number]
export const PaymentMethods = ["Card", "PayPal", "BankTransfer", "CashOnDelivery"] as const
export type PaymentMethod = (typeof PaymentMethods)[number]
export const ShippingMethods = ["Standard", "Express", "Pickup"] as const
export type ShippingMethod = (typeof ShippingMethods)[number]

export const statusBadge: Record<OrderStatus, string> = {
    Pending: "text-bg-warning",
    Paid: "text-bg-info",
    Processing: "text-bg-primary",
    Shipped: "text-bg-secondary",
    Delivered: "text-bg-success",
    Cancelled: "text-bg-danger",
}

export class Order extends EntityBase {
    id: number = 0
    code?: string // server-owned, minted on create
    status: OrderStatus = "Pending"
    customerName = ""
    email = ""
    phone?: string
    street = ""
    postalCode = ""
    city = ""
    country = "Belgium"
    shippingMethod: ShippingMethod = "Standard"
    paymentMethod: PaymentMethod = "Card"
    notes?: string
    // computed server-side
    itemCount = 0
    subtotal = 0
    shippingCost = 0
    total = 0

    orderLines?: Array<OrderLine>

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.code ?? (this.id ? `#${this.id}` : undefined)
    }
}

export const Entity = Order
export default Order
