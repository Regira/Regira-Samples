import { defineStore } from "pinia"
import type { ProductDto } from "@/types/models"

export interface CartLine {
  productId: number
  title: string
  slug: string | null
  imageUrl: string | null
  price: number
  stock: number
  quantity: number
}

interface CartState {
  lines: CartLine[]
}

const STORAGE_KEY = "webshop.cart.v1"

function load(): CartLine[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? (JSON.parse(raw) as CartLine[]) : []
  } catch {
    return []
  }
}

export const useCartStore = defineStore("cart", {
  state: (): CartState => ({ lines: load() }),
  getters: {
    itemCount: (state) => state.lines.reduce((sum, l) => sum + l.quantity, 0),
    subTotal: (state) => state.lines.reduce((sum, l) => sum + l.price * l.quantity, 0),
    isEmpty: (state) => state.lines.length === 0,
  },
  actions: {
    persist() {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(this.lines))
    },
    add(product: ProductDto, quantity = 1) {
      const existing = this.lines.find((l) => l.productId === product.id)
      const maxQty = Math.max(product.stock, 0)
      if (existing) {
        existing.quantity = Math.min(existing.quantity + quantity, maxQty || existing.quantity + quantity)
      } else {
        this.lines.push({
          productId: product.id,
          title: product.title,
          slug: product.slug,
          imageUrl: product.imageUrl,
          price: product.price,
          stock: product.stock,
          quantity: Math.min(quantity, maxQty || quantity),
        })
      }
      this.persist()
    },
    setQuantity(productId: number, quantity: number) {
      const line = this.lines.find((l) => l.productId === productId)
      if (!line) return
      if (quantity <= 0) {
        this.remove(productId)
        return
      }
      line.quantity = Math.min(quantity, line.stock || quantity)
      this.persist()
    },
    remove(productId: number) {
      this.lines = this.lines.filter((l) => l.productId !== productId)
      this.persist()
    },
    clear() {
      this.lines = []
      this.persist()
    },
  },
})
