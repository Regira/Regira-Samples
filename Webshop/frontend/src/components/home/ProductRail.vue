<script setup lang="ts">
import type { ProductDto } from "@/types/models"
import ProductCard from "@/components/product/ProductCard.vue"

defineProps<{
  eyebrow: string
  title: string
  products: ProductDto[]
  loading?: boolean
  seeAllQuery?: Record<string, string>
}>()
</script>

<template>
  <section class="ws-container ws-rail">
    <div class="ws-section-head">
      <div>
        <span class="ws-eyebrow">{{ eyebrow }}</span>
        <h2 class="display-font">{{ title }}</h2>
      </div>
      <RouterLink :to="{ name: 'shop', query: seeAllQuery ?? {} }" class="ws-see-all">
        See all <i class="bi bi-arrow-right"></i>
      </RouterLink>
    </div>
    <div v-if="loading" class="ws-rail-grid">
      <div v-for="i in 4" :key="i" class="ws-skeleton" style="aspect-ratio: 3/4"></div>
    </div>
    <div v-else class="ws-rail-grid">
      <ProductCard v-for="p in products" :key="p.id" :product="p" />
    </div>
  </section>
</template>

<style scoped>
.ws-rail {
  padding: 2.5rem 0;
}
.ws-section-head {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  margin-bottom: 1.4rem;
  gap: 1rem;
  flex-wrap: wrap;
}
.ws-section-head h2 {
  margin: 0.25rem 0 0;
  font-size: 1.5rem;
}
.ws-see-all {
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--ws-primary);
  white-space: nowrap;
}
.ws-rail-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.25rem;
}
@media (max-width: 992px) {
  .ws-rail-grid {
    grid-template-columns: repeat(3, 1fr);
  }
}
@media (max-width: 700px) {
  .ws-rail-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>
