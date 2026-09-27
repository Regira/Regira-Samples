import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Product } from "@/entities/products"

// An owned child row of Order (back-end `e.Related(x => x.OrderLines)`). Rows are edited inside the
// parent form and persisted with the parent's single `save()`; removal is a `_deleted` mark, never a splice.
export class OrderLine extends EntityBase {
    id: number = 0 // real for existing rows; useOwnedCollection mints a negative temp id for new rows
    orderId?: number
    _deleted?: boolean // marked-for-removal — the parent's EntityService.prepareItem drops these before save

    productId?: number
    product?: Product
    quantity = 1
    // server-owned snapshots (never sent back as input: the API re-prices every line)
    productTitle?: string
    unitPrice?: number
    lineTotal?: number
    sortOrder = 0

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.productTitle ?? this.product?.title
    }

    /** Lift a plain JSON row into the model (used by the Order service's toEntity) */
    static create(values?: object): OrderLine {
        return Object.assign(new OrderLine(), values || {})
    }
}

export const Entity = OrderLine
export default OrderLine
