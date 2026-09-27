import { defineStore } from "pinia"
import { computed, ref, watch } from "vue"
import type { Entity as Product } from "@/entities/products"
import { FREE_SHIPPING_THRESHOLD } from "./money"

/** What the cart remembers of a product (a snapshot; refreshed from the API on the cart and checkout pages) */
export interface CartProduct {
    id: number
    title: string
    price: number
    compareAtPrice?: number
    stock: number
    isActive: boolean
    brandTitle?: string
    categoryId?: number
    categoryTitle?: string
    categoryIcon?: string
    categoryColor?: string
    imageUrl?: string
}
export interface CartLine {
    productId: number
    quantity: number
    product: CartProduct
}

const STORAGE_KEY = "nordlys.cart.v1"
export const MAX_QUANTITY = 99

function load(): Array<CartLine> {
    try {
        const raw = localStorage.getItem(STORAGE_KEY)
        return raw ? (JSON.parse(raw) as Array<CartLine>) : []
    } catch {
        return []
    }
}

export function toCartProduct(p: Product): CartProduct {
    return {
        id: p.id,
        title: p.title,
        price: p.price,
        compareAtPrice: p.compareAtPrice,
        stock: p.stock,
        isActive: p.isActive,
        brandTitle: p.brand?.title,
        categoryId: p.categoryId,
        categoryTitle: p.category?.title,
        categoryIcon: p.category?.icon,
        categoryColor: p.category?.color,
        imageUrl: p.imageUrl,
    }
}

export const useCartStore = defineStore("cart", () => {
    const lines = ref<Array<CartLine>>(load())
    watch(
        lines,
        (value) => {
            try {
                localStorage.setItem(STORAGE_KEY, JSON.stringify(value))
            } catch {
                /* storage unavailable (private mode) - the cart still works for this session */
            }
        },
        { deep: true }
    )

    const count = computed(() => lines.value.reduce((sum, l) => sum + l.quantity, 0))
    const subtotal = computed(() => Math.round(lines.value.reduce((sum, l) => sum + l.quantity * l.product.price, 0) * 100) / 100)
    const savings = computed(() =>
        lines.value.reduce((sum, l) => sum + (l.product.compareAtPrice && l.product.compareAtPrice > l.product.price ? (l.product.compareAtPrice - l.product.price) * l.quantity : 0), 0)
    )
    const freeShippingRemaining = computed(() => Math.max(0, FREE_SHIPPING_THRESHOLD - subtotal.value))
    const unavailableLines = computed(() => lines.value.filter((l) => !l.product.isActive || l.product.stock < l.quantity))

    function quantityOf(productId: number) {
        return lines.value.find((l) => l.productId === productId)?.quantity ?? 0
    }
    function add(product: Product, quantity = 1) {
        const line = lines.value.find((l) => l.productId === product.id)
        const max = Math.min(MAX_QUANTITY, product.stock)
        if (line) {
            line.quantity = Math.min(max, line.quantity + quantity)
            line.product = toCartProduct(product)
        } else {
            lines.value.push({ productId: product.id, quantity: Math.min(max, quantity), product: toCartProduct(product) })
        }
    }
    function setQuantity(productId: number, quantity: number) {
        const line = lines.value.find((l) => l.productId === productId)
        if (!line) return
        if (quantity <= 0) return remove(productId)
        line.quantity = Math.min(quantity, MAX_QUANTITY, Math.max(line.product.stock, 1))
    }
    function remove(productId: number) {
        lines.value = lines.value.filter((l) => l.productId !== productId)
    }
    function clear() {
        lines.value = []
    }
    /** Replace the snapshots with fresh product data (prices, stock, availability) */
    function refresh(products: Array<Product>) {
        for (const line of lines.value) {
            const fresh = products.find((p) => p.id === line.productId)
            if (fresh) line.product = toCartProduct(fresh)
            else line.product = { ...line.product, isActive: false, stock: 0 }
        }
    }

    return { lines, count, subtotal, savings, freeShippingRemaining, unavailableLines, quantityOf, add, setQuantity, remove, clear, refresh }
})
