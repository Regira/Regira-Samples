import { ref } from "vue"
import { useEntityStore as useCategoryStore, type Entity as Category } from "@/entities/categories"
import { useEntityStore as useBrandStore, type Entity as Brand } from "@/entities/brands"

// Storefront reference data, loaded once per session through the pooled stores
const categories = ref<Array<Category>>([])
const brands = ref<Array<Brand>>([])
let loading: Promise<void> | undefined

export function useShopCatalog() {
    const categoryStore = useCategoryStore()
    const brandStore = useBrandStore()

    function load(): Promise<void> {
        loading ??= Promise.all([categoryStore.service.list({ pageSize: 0 }), brandStore.service.list({ pageSize: 0 })])
            .then(([c, b]) => {
                categories.value = categoryStore.fromPool(c)
                brands.value = brandStore.fromPool(b)
            })
            .catch((ex) => {
                console.error("loading the catalog failed", ex)
                loading = undefined
            })
        return loading
    }
    const categoryById = (id?: number | string) => categories.value.find((c) => c.id == id)
    const brandById = (id?: number | string) => brands.value.find((b) => b.id == id)

    return { categories, brands, load, categoryById, brandById }
}
