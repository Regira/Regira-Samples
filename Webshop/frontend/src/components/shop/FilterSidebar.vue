<script setup lang="ts">
import type { CategoryDto } from "@/types/models"

export interface ShopFilters {
  categoryIds: number[]
  brands: string[]
  minPrice: number | null
  maxPrice: number | null
  inStockOnly: boolean
  onSale: boolean
  featured: boolean
}

const props = defineProps<{
  modelValue: ShopFilters
  categories: CategoryDto[]
  brands: string[]
  priceCeiling: number
}>()
const emit = defineEmits<{ (e: "update:modelValue", value: ShopFilters): void; (e: "clear"): void }>()

function patch(partial: Partial<ShopFilters>) {
  emit("update:modelValue", { ...props.modelValue, ...partial })
}

function toggleCategory(id: number) {
  const set = new Set(props.modelValue.categoryIds)
  set.has(id) ? set.delete(id) : set.add(id)
  patch({ categoryIds: [...set] })
}

function toggleBrand(name: string) {
  const set = new Set(props.modelValue.brands)
  set.has(name) ? set.delete(name) : set.add(name)
  patch({ brands: [...set] })
}
</script>

<template>
  <aside class="ws-filters">
    <div class="ws-filters-head">
      <h6>Filters</h6>
      <button type="button" class="ws-clear-btn" @click="emit('clear')">Clear all</button>
    </div>

    <div class="ws-filter-group">
      <h6>Category</h6>
      <label v-for="cat in categories" :key="cat.id" class="ws-check">
        <input type="checkbox" :checked="modelValue.categoryIds.includes(cat.id)" @change="toggleCategory(cat.id)" />
        <span>{{ cat.title }}</span>
        <span class="ws-check-count">{{ cat.productCount }}</span>
      </label>
    </div>

    <div class="ws-filter-group">
      <h6>Price</h6>
      <div class="ws-price-range">
        <span>&euro;{{ modelValue.minPrice ?? 0 }}</span>
        <span>&euro;{{ modelValue.maxPrice ?? priceCeiling }}</span>
      </div>
      <input
        type="range"
        class="ws-range"
        min="0"
        :max="priceCeiling"
        :value="modelValue.maxPrice ?? priceCeiling"
        @input="patch({ maxPrice: Number(($event.target as HTMLInputElement).value) })"
      />
    </div>

    <div class="ws-filter-group">
      <h6>Brand</h6>
      <div class="ws-brand-scroll">
        <label v-for="b in brands" :key="b" class="ws-check">
          <input type="checkbox" :checked="modelValue.brands.includes(b)" @change="toggleBrand(b)" />
          <span>{{ b }}</span>
        </label>
      </div>
    </div>

    <div class="ws-filter-group">
      <h6>Availability</h6>
      <label class="ws-check">
        <input type="checkbox" :checked="modelValue.inStockOnly" @change="patch({ inStockOnly: !modelValue.inStockOnly })" />
        <span>In stock only</span>
      </label>
      <label class="ws-check">
        <input type="checkbox" :checked="modelValue.onSale" @change="patch({ onSale: !modelValue.onSale })" />
        <span>On sale</span>
      </label>
      <label class="ws-check">
        <input type="checkbox" :checked="modelValue.featured" @change="patch({ featured: !modelValue.featured })" />
        <span>Featured</span>
      </label>
    </div>
  </aside>
</template>

<style scoped>
.ws-filters {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}
.ws-filters-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.ws-filters-head h6 {
  font-weight: 800;
  font-size: 1rem;
  margin: 0;
}
.ws-clear-btn {
  border: none;
  background: none;
  color: var(--ws-primary);
  font-size: 0.8rem;
  font-weight: 600;
  cursor: pointer;
  padding: 0;
}
.ws-filter-group {
  border-top: 1px solid var(--ws-line);
  padding-top: 1rem;
}
.ws-filter-group h6 {
  font-size: 0.82rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--ws-ink-soft);
  margin-bottom: 0.7rem;
}
.ws-check {
  display: flex;
  align-items: center;
  gap: 0.55rem;
  font-size: 0.88rem;
  padding: 0.3rem 0;
  cursor: pointer;
  color: var(--ws-ink);
}
.ws-check input {
  accent-color: var(--ws-primary);
  width: 1rem;
  height: 1rem;
}
.ws-check span:first-of-type {
  flex: 1 1 auto;
}
.ws-check-count {
  color: var(--ws-muted);
  font-size: 0.76rem;
}
.ws-price-range {
  display: flex;
  justify-content: space-between;
  font-size: 0.82rem;
  color: var(--ws-ink-soft);
  margin-bottom: 0.4rem;
  font-weight: 600;
}
.ws-range {
  width: 100%;
  accent-color: var(--ws-primary);
}
.ws-brand-scroll {
  max-height: 220px;
  overflow-y: auto;
  padding-right: 0.3rem;
}
</style>
