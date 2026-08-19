<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import type { CategoryDto, ProductDto } from "@/types/models"
import { fetchCategories } from "@/api/categories"
import { searchProducts, type ProductSortBy } from "@/api/products"
import ProductCard from "@/components/product/ProductCard.vue"
import Pagination from "@/components/common/Pagination.vue"
import FilterSidebar, { type ShopFilters } from "@/components/shop/FilterSidebar.vue"

const BRANDS = [
  "Northwind", "Zenlyte", "Vertex", "Boreal", "Cascade", "Lumino", "Kestrel", "Marlow",
  "Ashgrove", "Pinehall", "Solace", "Redwick", "Havenly", "Crestline", "Amberwood",
]
const PRICE_CEILING = 900
const PAGE_SIZE = 12

const route = useRoute()
const router = useRouter()

const categories = ref<CategoryDto[]>([])
const products = ref<ProductDto[]>([])
const count = ref(0)
const loading = ref(true)
const page = ref(1)
const sortBy = ref<ProductSortBy>("Default")
const q = ref((route.query.q as string) ?? "")
const mobileFiltersOpen = ref(false)

const filters = ref<ShopFilters>({
  categoryIds: [],
  brands: [],
  minPrice: null,
  maxPrice: null,
  inStockOnly: false,
  onSale: route.query.onSale === "1",
  featured: route.query.featured === "1",
})

const activeCategory = computed(() => {
  const slug = route.params.categorySlug as string | undefined
  if (!slug) return null
  return categories.value.find((c) => c.slug === slug) ?? null
})

async function load() {
  loading.value = true
  try {
    const categoryIds = [...filters.value.categoryIds]
    if (activeCategory.value) categoryIds.push(activeCategory.value.id)
    const result = await searchProducts({
      q: q.value || undefined,
      categoryId: categoryIds.length ? [...new Set(categoryIds)] : undefined,
      brand: filters.value.brands.length ? filters.value.brands : undefined,
      minPrice: filters.value.minPrice ?? undefined,
      maxPrice: filters.value.maxPrice ?? undefined,
      inStockOnly: filters.value.inStockOnly || undefined,
      onSale: filters.value.onSale || undefined,
      isFeatured: filters.value.featured || undefined,
      page: page.value,
      pageSize: PAGE_SIZE,
      sortBy: sortBy.value,
    })
    products.value = result.items
    count.value = result.count
  } finally {
    loading.value = false
  }
}

function clearFilters() {
  filters.value = {
    categoryIds: [],
    brands: [],
    minPrice: null,
    maxPrice: null,
    inStockOnly: false,
    onSale: false,
    featured: false,
  }
  if (route.params.categorySlug) router.push({ name: "shop" })
}

onMounted(async () => {
  categories.value = await fetchCategories()
  await load()
})

watch([filters, sortBy, q, () => route.params.categorySlug], () => {
  page.value = 1
  load()
}, { deep: true })

watch(page, load)

watch(
  () => route.query,
  (query) => {
    if (query.onSale === "1" && !filters.value.onSale) filters.value.onSale = true
    if (query.featured === "1" && !filters.value.featured) filters.value.featured = true
    if (typeof query.q === "string") q.value = query.q
  },
)
</script>

<template>
  <div class="ws-container ws-shop">
    <div class="ws-shop-head">
      <div>
        <span class="ws-eyebrow">{{ activeCategory ? activeCategory.title : "All products" }}</span>
        <h1 class="display-font">{{ activeCategory ? activeCategory.title : "Shop the full collection" }}</h1>
        <p v-if="activeCategory?.description" class="ws-muted">{{ activeCategory.description }}</p>
      </div>
    </div>

    <div class="ws-shop-toolbar">
      <button type="button" class="btn-ws btn-ws-outline btn-ws-sm d-lg-none" @click="mobileFiltersOpen = !mobileFiltersOpen">
        <i class="bi bi-sliders"></i> Filters
      </button>
      <span class="ws-result-count ws-muted">{{ loading ? "Loading..." : `${count} product${count === 1 ? "" : "s"}` }}</span>
      <select v-model="sortBy" class="ws-select ws-sort-select">
        <option value="Default">Sort: Featured</option>
        <option value="Newest">Newest</option>
        <option value="Price">Price: Low to High</option>
        <option value="PriceDesc">Price: High to Low</option>
        <option value="Rating">Top Rated</option>
        <option value="Title">Name: A-Z</option>
      </select>
    </div>

    <div class="ws-shop-layout">
      <div class="ws-shop-sidebar" :class="{ 'ws-shop-sidebar-open': mobileFiltersOpen }">
        <FilterSidebar
          v-model="filters"
          :categories="categories"
          :brands="BRANDS"
          :price-ceiling="PRICE_CEILING"
          @clear="clearFilters"
        />
      </div>

      <div class="ws-shop-results">
        <div v-if="loading" class="ws-product-grid">
          <div v-for="i in 9" :key="i" class="ws-skeleton" style="aspect-ratio: 3/4"></div>
        </div>
        <template v-else>
          <div v-if="products.length === 0" class="ws-empty">
            <i class="bi bi-emoji-frown"></i>
            <h3>No products match your filters</h3>
            <p class="ws-muted">Try widening your search or clearing some filters.</p>
            <button type="button" class="btn-ws btn-ws-outline" @click="clearFilters">Clear filters</button>
          </div>
          <div v-else class="ws-product-grid">
            <ProductCard v-for="p in products" :key="p.id" :product="p" />
          </div>
          <Pagination v-if="products.length" v-model:page="page" :page-size="PAGE_SIZE" :count="count" class="mt-4" />
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ws-shop {
  padding: 2.5rem 1.5rem 4rem;
}
.ws-shop-head h1 {
  font-size: 1.9rem;
  margin: 0.25rem 0 0.5rem;
}
.ws-shop-toolbar {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin: 1.6rem 0 1.4rem;
  padding-bottom: 1.2rem;
  border-bottom: 1px solid var(--ws-line);
}
.ws-result-count {
  font-size: 0.86rem;
  flex: 1 1 auto;
}
.ws-sort-select {
  width: auto;
  max-width: 200px;
}
.ws-shop-layout {
  display: grid;
  grid-template-columns: 240px 1fr;
  gap: 2.2rem;
  align-items: start;
}
@media (max-width: 992px) {
  .ws-shop-layout {
    grid-template-columns: 1fr;
  }
  .ws-shop-sidebar {
    display: none;
  }
  .ws-shop-sidebar-open {
    display: block;
    background: var(--ws-surface);
    border: 1px solid var(--ws-line);
    border-radius: var(--ws-radius);
    padding: 1.2rem;
    margin-bottom: 1rem;
  }
}
.ws-product-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 1.25rem;
}
@media (max-width: 700px) {
  .ws-product-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
.ws-empty {
  text-align: center;
  padding: 4rem 1rem;
}
.ws-empty i {
  font-size: 2.4rem;
  color: var(--ws-muted);
}
.ws-empty h3 {
  margin: 0.8rem 0 0.3rem;
  font-size: 1.15rem;
}
.ws-empty .btn-ws {
  margin-top: 1rem;
}
</style>
