import { computed, ref } from "vue"
import { defineStore } from "pinia"
import { useEntityStore as useShopperStore, type Entity as Shopper } from "@/entities/shoppers"

const STORAGE_KEY = "shopmate.shopperId"

function readStoredId(): number | undefined {
    try {
        const value = Number(localStorage.getItem(STORAGE_KEY))
        return value > 0 ? value : undefined
    } catch {
        return undefined
    }
}

/**
 * The shopper this device is shopping as (no sign-in in this app: the shopper is picked, not authenticated).
 * Persisted per device; falls back to the first shopper.
 */
export const useCurrentShopper = defineStore("currentShopper", () => {
    const shopperStore = useShopperStore()
    const shopperId = ref<number | undefined>(readStoredId())
    const shoppers = ref<Array<Shopper>>([])
    const isLoaded = ref(false)

    const shopper = computed(() => shoppers.value.find((x) => x.id === shopperId.value))

    async function load() {
        const items = await shopperStore.service.list({ pageSize: 0 })
        shoppers.value = shopperStore.fromPool(items)
        if (!shoppers.value.some((x) => x.id === shopperId.value)) select(shoppers.value[0])
        isLoaded.value = true
    }

    function select(value?: Shopper) {
        shopperId.value = value?.id
        try {
            if (value?.id) localStorage.setItem(STORAGE_KEY, String(value.id))
            else localStorage.removeItem(STORAGE_KEY)
        } catch {
            /* storage unavailable: selection lives for this session only */
        }
    }

    return { shopperId, shopper, shoppers, isLoaded, load, select }
})

export default useCurrentShopper
