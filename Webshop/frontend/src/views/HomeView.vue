<script setup lang="ts">
import { onMounted, ref } from "vue"
import type { CategoryDto, ProductDto } from "@/types/models"
import { fetchCategories } from "@/api/categories"
import { searchProducts } from "@/api/products"
import HeroBanner from "@/components/home/HeroBanner.vue"
import UspStrip from "@/components/home/UspStrip.vue"
import CategoryGrid from "@/components/home/CategoryGrid.vue"
import PromoBanner from "@/components/home/PromoBanner.vue"
import ProductRail from "@/components/home/ProductRail.vue"

const categories = ref<CategoryDto[]>([])
const featured = ref<ProductDto[]>([])
const onSale = ref<ProductDto[]>([])
const loadingFeatured = ref(true)
const loadingSale = ref(true)

onMounted(async () => {
  fetchCategories().then((cats) => {
    categories.value = cats.sort((a, b) => a.displayOrder - b.displayOrder)
  })
  searchProducts({ isFeatured: true, pageSize: 8, sortBy: "Rating" })
    .then((r) => (featured.value = r.items))
    .finally(() => (loadingFeatured.value = false))
  searchProducts({ onSale: true, pageSize: 4, sortBy: "Newest" })
    .then((r) => (onSale.value = r.items))
    .finally(() => (loadingSale.value = false))
})
</script>

<template>
  <div>
    <HeroBanner />
    <UspStrip />
    <CategoryGrid :categories="categories" />
    <PromoBanner />
    <ProductRail
      eyebrow="Hand-picked"
      title="Featured products"
      :products="featured"
      :loading="loadingFeatured"
      :see-all-query="{ featured: '1' }"
    />
    <ProductRail
      eyebrow="Don't miss out"
      title="On sale right now"
      :products="onSale"
      :loading="loadingSale"
      :see-all-query="{ onSale: '1' }"
    />
  </div>
</template>
