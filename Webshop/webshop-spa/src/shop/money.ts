import { formatCurrency } from "@regira/modules/vue/formatters"
import { useConfig } from "@/app-config"

/** Formats an amount in the store's currency and culture (config.json → culture / currency) */
export function money(value?: number): string {
    const { culture, currency } = useConfig()
    return formatCurrency(value ?? 0, culture || "en-IE", currency || "EUR")
}

// Display mirror of the API's OrderPricing — the server recomputes every amount on save.
export const FREE_SHIPPING_THRESHOLD = 50
export const shippingOptions = [
    { key: "Standard", title: "Standard delivery", eta: "2-4 business days", icon: "bi bi-truck", price: 4.95 },
    { key: "Express", title: "Express delivery", eta: "Next business day", icon: "bi bi-lightning-charge", price: 9.95 },
    { key: "Pickup", title: "Pick up in store", eta: "Ready in 2 hours", icon: "bi bi-shop", price: 0 },
] as const
export type ShippingKey = (typeof shippingOptions)[number]["key"]

export function shippingCost(method: ShippingKey, subtotal: number, itemCount: number): number {
    if (itemCount === 0) return 0
    if (method === "Express") return 9.95
    if (method === "Pickup") return 0
    return subtotal >= FREE_SHIPPING_THRESHOLD ? 0 : 4.95
}

export const paymentOptions = [
    { key: "Card", title: "Credit or debit card", icon: "bi bi-credit-card-2-front" },
    { key: "PayPal", title: "PayPal", icon: "bi bi-paypal" },
    { key: "BankTransfer", title: "Bank transfer", icon: "bi bi-bank" },
    { key: "CashOnDelivery", title: "Cash on delivery", icon: "bi bi-cash-coin" },
] as const

export const countries = ["Belgium", "Netherlands", "Luxembourg", "Germany", "France"] as const
