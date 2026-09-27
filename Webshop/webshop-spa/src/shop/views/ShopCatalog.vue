<!-- Product catalog: server-side filtering, sorting and paging, all mirrored in the URL (deep-linkable) -->
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { RouterLink } from "vue-router"
import { useSearchView, useRouteOverview } from "@regira/modules/vue/entities"
import { Paging, Feedback } from "@regira/modules/vue/ui"
import { useEntityStore as useProductStore, type Entity as Product } from "@/entities/products"
import ProductSearchObject from "@/entities/products/filter/SearchObject"
import { useShopCatalog } from "../catalog"
import { money } from "../money"
import ProductCard from "../components/ProductCard.vue"

const PAGE_SIZE = 24
const sortOptions = [
    { value: "", title: "Recommended" },
    { value: "Newest", title: "Newest" },
    { value: "PriceAsc", title: "Price: low to high" },
    { value: "PriceDesc", title: "Price: high to low" },
    { value: "Rating", title: "Top rated" },
    { value: "Popularity", title: "Most reviewed" },
]
const priceRanges = [
    { title: `Under ${money(25)}`, min: undefined, max: 25 },
    { title: `${money(25)} - ${money(100)}`, min: 25, max: 100 },
    { title: `${money(100)} - ${money(500)}`, min: 100, max: 500 },
    { title: `Over ${money(500)}`, min: 500, max: undefined },
]

const { service, fromPool } = useProductStore()
const { categories, brands, load, categoryById, brandById } = useShopCatalog()

// the storefront only ever lists published products: every search from this view adds isActive=true
const shopService = Object.assign(Object.create(service), { search: (so: any) => service.search({ ...so, isActive: true }) }) as typeof service
const { searchObject, pagingInfo, items, itemsCount, isLoading, feedback, searchHandler } = useSearchView<Product, ProductSearchObject>({
    service: shopService,
    searchObject: new ProductSearchObject(),
    defaultPageSize: PAGE_SIZE,
})
const { updateOverviewRoute } = useRouteOverview({ searchObject, pagingInfo, handler: searchHandler, defaultPageSize: PAGE_SIZE })
onMounted(load)

// route values arrive as strings (or arrays of strings) - normalise them for the controls
const so = computed(() => searchObject.value as Record<string, any>)
const asArray = (v: unknown): Array<string> => (v == null || v === "" ? [] : Array.isArray(v) ? v.map(String) : [String(v)])
const isTrue = (v: unknown) => String(v) === "true"
const selectedCategory = computed(() => categoryById(asArray(so.value.categoryId)[0]))
const selectedBrandIds = computed(() => asArray(so.value.brandId))
const sortBy = computed(() => asArray(so.value.sortBy)[0] ?? "")
const minPrice = ref<number>()
const maxPrice = ref<number>()
watch(
    () => [so.value.minPrice, so.value.maxPrice],
    ([min, max]) => {
        minPrice.value = min != null && min !== "" ? Number(min) : undefined
        maxPrice.value = max != null && max !== "" ? Number(max) : undefined
    },
    { immediate: true }
)
const showFilters = ref(false)

function apply(changes: Record<string, any>) {
    const next: Record<string, any> = { ...so.value, ...changes }
    for (const key of Object.keys(next)) if (next[key] === "" || next[key] === false || (Array.isArray(next[key]) && !next[key].length)) next[key] = undefined
    searchObject.value = next as ProductSearchObject
    updateOverviewRoute(true)
}
const setCategory = (id?: number) => apply({ categoryId: id, brandId: undefined })
function toggleBrand(id: number) {
    const current = selectedBrandIds.value
    apply({ brandId: current.includes(String(id)) ? current.filter((b) => b !== String(id)) : [...current, String(id)] })
}
const setPrice = (min?: number, max?: number) => apply({ minPrice: min, maxPrice: max })
const clearAll = () => {
    searchObject.value = new ProductSearchObject()
    apply({})
}

const title = computed(() => {
    if (so.value.q) return `Results for "${so.value.q}"`
    if (selectedCategory.value) return selectedCategory.value.title
    if (isTrue(so.value.onSale)) return "Deals"
    if (isTrue(so.value.isFeatured)) return "Staff picks"
    return "All products"
})
const activeChips = computed(() => {
    const chips: Array<{ label: string; clear: () => void }> = []
    if (so.value.q) chips.push({ label: `"${so.value.q}"`, clear: () => apply({ q: undefined }) })
    if (selectedCategory.value) chips.push({ label: selectedCategory.value.title, clear: () => setCategory(undefined) })
    for (const id of selectedBrandIds.value) chips.push({ label: brandById(id)?.title ?? `Brand ${id}`, clear: () => toggleBrand(Number(id)) })
    if (so.value.minPrice || so.value.maxPrice)
        chips.push({
            label: `${so.value.minPrice ? money(Number(so.value.minPrice)) : money(0)} - ${so.value.maxPrice ? money(Number(so.value.maxPrice)) : "max"}`,
            clear: () => setPrice(),
        })
    if (so.value.minRating) chips.push({ label: `${so.value.minRating}+ stars`, clear: () => apply({ minRating: undefined }) })
    if (isTrue(so.value.onSale)) chips.push({ label: "On sale", clear: () => apply({ onSale: undefined }) })
    if (isTrue(so.value.inStock)) chips.push({ label: "In stock", clear: () => apply({ inStock: undefined }) })
    if (isTrue(so.value.isFeatured)) chips.push({ label: "Staff picks", clear: () => apply({ isFeatured: undefined }) })
    return chips
})
const products = computed(() => fromPool(items.value ?? []))
const fromIndex = computed(() => ((pagingInfo.value?.page ?? 1) - 1) * (pagingInfo.value?.pageSize ?? PAGE_SIZE) + 1)
const toIndex = computed(() => fromIndex.value + (items.value?.length ?? 0) - 1)
</script>

<template>
    <div class="container-xl py-3 py-md-4">
        <nav aria-label="breadcrumb">
            <ol class="breadcrumb small">
                <li class="breadcrumb-item"><RouterLink :to="{ name: 'home' }">Home</RouterLink></li>
                <li class="breadcrumb-item" :class="{ active: !selectedCategory }">
                    <RouterLink v-if="selectedCategory" :to="{ name: 'shop' }">Shop</RouterLink><span v-else>Shop</span>
                </li>
                <li v-if="selectedCategory" class="breadcrumb-item active">{{ selectedCategory.title }}</li>
            </ol>
        </nav>

        <div class="ws-catalog-head" :style="selectedCategory ? { '--cat': selectedCategory.color } : {}">
            <div>
                <h1 class="ws-page-title">
                    <i v-if="selectedCategory" :class="selectedCategory.$iconClass" class="me-2 ws-cat-color"></i>{{ title }}
                </h1>
                <p class="text-body-secondary mb-0">
                    {{ selectedCategory?.description ?? "Everything we sell, in one place." }}
                </p>
            </div>
        </div>

        <div class="row g-4">
            <!-- filter sidebar (collapsible on small screens) -->
            <aside class="col-lg-3">
                <button type="button" class="btn btn-outline-secondary w-100 d-lg-none mb-2" @click="showFilters = !showFilters">
                    <i class="bi bi-sliders me-1"></i>{{ showFilters ? "Hide filters" : "Show filters" }}
                    <span v-if="activeChips.length" class="badge text-bg-primary ms-1">{{ activeChips.length }}</span>
                </button>
                <div class="ws-filters" :class="{ 'd-none d-lg-block': !showFilters }">
                    <form class="ws-filter-block" @submit.prevent="apply({ q: ($refs.qInput as HTMLInputElement).value.trim() || undefined })">
                        <div class="input-group">
                            <input ref="qInput" type="search" class="form-control" :value="so.q" placeholder="Search within results" aria-label="Keywords" />
                            <button class="btn btn-outline-secondary" type="submit" aria-label="Search"><i class="bi bi-search"></i></button>
                        </div>
                    </form>

                    <div class="ws-filter-block">
                        <h6>Category</h6>
                        <button type="button" class="ws-filter-option" :class="{ active: !selectedCategory }" @click="setCategory(undefined)">
                            <span><i class="bi bi-grid me-2"></i>All categories</span>
                        </button>
                        <button
                            v-for="c in categories"
                            :key="c.id"
                            type="button"
                            class="ws-filter-option"
                            :class="{ active: selectedCategory?.id === c.id }"
                            @click="setCategory(c.id)"
                        >
                            <span><i :class="c.$iconClass" class="me-2" :style="{ color: c.color }"></i>{{ c.title }}</span>
                            <small class="text-body-secondary">{{ c.productCount }}</small>
                        </button>
                    </div>

                    <div class="ws-filter-block">
                        <h6>Price</h6>
                        <button
                            v-for="r in priceRanges"
                            :key="r.title"
                            type="button"
                            class="ws-filter-option"
                            :class="{ active: minPrice === r.min && maxPrice === r.max }"
                            @click="setPrice(r.min, r.max)"
                        >
                            {{ r.title }}
                        </button>
                        <form class="d-flex gap-2 mt-2 align-items-center" @submit.prevent="setPrice(minPrice || undefined, maxPrice || undefined)">
                            <input v-model.number="minPrice" type="number" min="0" class="form-control form-control-sm" placeholder="Min" aria-label="Minimum price" />
                            <span>-</span>
                            <input v-model.number="maxPrice" type="number" min="0" class="form-control form-control-sm" placeholder="Max" aria-label="Maximum price" />
                            <button class="btn btn-sm btn-outline-primary" type="submit">Go</button>
                        </form>
                    </div>

                    <div class="ws-filter-block">
                        <h6>Customer rating</h6>
                        <button
                            v-for="r in [4.5, 4, 3]"
                            :key="r"
                            type="button"
                            class="ws-filter-option"
                            :class="{ active: Number(so.minRating) === r }"
                            @click="apply({ minRating: Number(so.minRating) === r ? undefined : r })"
                        >
                            <span><i class="bi bi-star-fill text-warning me-1"></i>{{ r }} &amp; up</span>
                        </button>
                    </div>

                    <div class="ws-filter-block">
                        <h6>Availability</h6>
                        <div class="form-check form-switch">
                            <input id="f-sale" class="form-check-input" type="checkbox" :checked="isTrue(so.onSale)" @change="apply({ onSale: ($event.target as HTMLInputElement).checked || undefined })" />
                            <label class="form-check-label" for="f-sale">On sale</label>
                        </div>
                        <div class="form-check form-switch">
                            <input id="f-stock" class="form-check-input" type="checkbox" :checked="isTrue(so.inStock)" @change="apply({ inStock: ($event.target as HTMLInputElement).checked || undefined })" />
                            <label class="form-check-label" for="f-stock">In stock only</label>
                        </div>
                        <div class="form-check form-switch">
                            <input id="f-feat" class="form-check-input" type="checkbox" :checked="isTrue(so.isFeatured)" @change="apply({ isFeatured: ($event.target as HTMLInputElement).checked || undefined })" />
                            <label class="form-check-label" for="f-feat">Staff picks</label>
                        </div>
                    </div>

                    <div class="ws-filter-block">
                        <h6>Brand</h6>
                        <div class="ws-brand-list">
                            <div v-for="b in brands" :key="b.id" class="form-check">
                                <input :id="`brand-${b.id}`" class="form-check-input" type="checkbox" :checked="selectedBrandIds.includes(String(b.id))" @change="toggleBrand(b.id)" />
                                <label class="form-check-label" :for="`brand-${b.id}`">{{ b.title }}</label>
                            </div>
                        </div>
                    </div>
                </div>
            </aside>

            <!-- results -->
            <section class="col-lg-9">
                <div class="ws-toolbar">
                    <span class="text-body-secondary small">
                        <template v-if="itemsCount">Showing {{ fromIndex }}-{{ toIndex }} of <strong>{{ itemsCount }}</strong> products</template>
                        <template v-else-if="!isLoading">No products found</template>
                    </span>
                    <div class="d-flex align-items-center gap-2">
                        <label for="sort" class="small text-body-secondary text-nowrap">Sort by</label>
                        <select id="sort" class="form-select form-select-sm" :value="sortBy" @change="apply({ sortBy: ($event.target as HTMLSelectElement).value || undefined })">
                            <option v-for="o in sortOptions" :key="o.value" :value="o.value">{{ o.title }}</option>
                        </select>
                    </div>
                </div>

                <div v-if="activeChips.length" class="ws-chips">
                    <button v-for="chip in activeChips" :key="chip.label" type="button" class="ws-chip" @click="chip.clear">
                        {{ chip.label }} <i class="bi bi-x"></i>
                    </button>
                    <button type="button" class="btn btn-link btn-sm" @click="clearAll">Clear all</button>
                </div>

                <Feedback :feedback="feedback" :hide-close-button="true" />

                <div class="ws-grid" :class="{ 'is-loading': isLoading }">
                    <template v-if="items == null">
                        <div v-for="i in 8" :key="i"><div class="ws-skeleton"></div></div>
                    </template>
                    <div v-for="p in products" :key="p.id"><ProductCard :product="p" /></div>
                </div>

                <div v-if="items && !items.length && !isLoading" class="ws-empty">
                    <i class="bi bi-search"></i>
                    <h5>No products match these filters</h5>
                    <p class="text-body-secondary">Try removing a filter or searching for something else.</p>
                    <button type="button" class="btn btn-primary" @click="clearAll">Clear all filters</button>
                </div>

                <div v-if="pagingInfo && itemsCount != null && itemsCount > (pagingInfo.pageSize ?? PAGE_SIZE)" class="d-flex justify-content-center mt-4">
                    <Paging v-model="pagingInfo" :count="itemsCount" @change="updateOverviewRoute(false)" />
                </div>
            </section>
        </div>
    </div>
</template>
