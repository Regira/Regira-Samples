import { EntityBase } from "@regira/modules/vue/entities"
import type { Entity as Category } from "@/entities/categories"
import type { Entity as Brand } from "@/entities/brands"

export class Product extends EntityBase {
    id: number = 0
    title = ""
    sku = ""
    description?: string
    price = 0
    compareAtPrice?: number // original price; higher than price = on sale
    stock = 0
    rating = 0 // 0-5
    reviewCount = 0
    imageUrl?: string
    isFeatured = false
    isActive = true
    categoryId?: number
    category?: Category // eager-loaded by the API (to-one on every row)
    brandId?: number
    brand?: Brand // eager-loaded by the API (to-one on every row)

    created?: Date
    lastModified?: Date

    override get $id(): string | number {
        return this.id || "new"
    }
    override get $title(): string | undefined {
        return this.title
    }
    get $isOnSale(): boolean {
        return this.compareAtPrice != null && this.compareAtPrice > this.price
    }
    get $discountPercentage(): number {
        return this.$isOnSale ? Math.round((1 - this.price / this.compareAtPrice!) * 100) : 0
    }
    get $isNew(): boolean {
        return this.created != null && Date.now() - this.created.getTime() < 30 * 24 * 3600 * 1000
    }
}

export const Entity = Product
export default Product
