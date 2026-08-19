<script setup lang="ts">
import { computed } from "vue"
import type { ProductDto } from "@/types/models"
import RatingStars from "./RatingStars.vue"
import PriceTag from "./PriceTag.vue"
import { useCartStore } from "@/stores/cart"
import { useToastStore } from "@/stores/toast"

const props = defineProps<{ product: ProductDto }>()
const cart = useCartStore()
const toast = useToastStore()

const onSale = computed(() => !!props.product.compareAtPrice && props.product.compareAtPrice > props.product.price)
const outOfStock = computed(() => props.product.stock <= 0)
const lowStock = computed(() => !outOfStock.value && props.product.stock <= 5)

function addToCart(e: Event) {
  e.preventDefault()
  e.stopPropagation()
  if (outOfStock.value) return
  cart.add(props.product, 1)
  toast.success(`Added "${props.product.title}" to cart`)
}
</script>

<template>
  <RouterLink :to="{ name: 'product-detail', params: { slug: product.slug ?? String(product.id) } }" class="ws-product-card">
    <div class="ws-product-media">
      <img :src="product.imageUrl ?? ''" :alt="product.title" loading="lazy" />
      <div class="ws-product-badges">
        <span v-if="onSale" class="ws-badge ws-badge-sale">Sale</span>
        <span v-else-if="product.isFeatured" class="ws-badge ws-badge-new">Featured</span>
        <span v-if="outOfStock" class="ws-badge ws-badge-out">Out of stock</span>
        <span v-else-if="lowStock" class="ws-badge ws-badge-low">Only {{ product.stock }} left</span>
      </div>
      <button
        type="button"
        class="ws-quick-add"
        :disabled="outOfStock"
        :title="outOfStock ? 'Out of stock' : 'Add to cart'"
        @click="addToCart"
      >
        <i class="bi bi-bag-plus"></i>
      </button>
    </div>
    <div class="ws-product-body">
      <span class="ws-product-brand">{{ product.brand ?? product.category?.title }}</span>
      <h3 class="ws-product-title">{{ product.title }}</h3>
      <RatingStars :rating="product.rating" :review-count="product.reviewCount" size="sm" />
      <PriceTag :price="product.price" :compare-at-price="product.compareAtPrice" class="mt-1" />
    </div>
  </RouterLink>
</template>

<style scoped>
.ws-product-card {
  display: flex;
  flex-direction: column;
  background: var(--ws-surface);
  border-radius: var(--ws-radius);
  border: 1px solid var(--ws-line);
  overflow: hidden;
  transition: box-shadow 0.16s ease, transform 0.16s ease;
  height: 100%;
}
.ws-product-card:hover {
  box-shadow: var(--ws-shadow);
  transform: translateY(-3px);
}
.ws-product-media {
  position: relative;
  aspect-ratio: 1 / 1;
  background: var(--ws-bg-alt);
  overflow: hidden;
}
.ws-product-media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  transition: transform 0.35s ease;
}
.ws-product-card:hover .ws-product-media img {
  transform: scale(1.05);
}
.ws-product-badges {
  position: absolute;
  top: 0.6rem;
  left: 0.6rem;
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  align-items: flex-start;
}
.ws-quick-add {
  position: absolute;
  bottom: 0.6rem;
  right: 0.6rem;
  width: 2.4rem;
  height: 2.4rem;
  border-radius: 50%;
  border: none;
  background: #fff;
  color: var(--ws-ink);
  box-shadow: var(--ws-shadow-sm);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  opacity: 0;
  transform: translateY(6px);
  transition: opacity 0.15s ease, transform 0.15s ease, background-color 0.15s ease, color 0.15s ease;
}
.ws-product-card:hover .ws-quick-add {
  opacity: 1;
  transform: translateY(0);
}
.ws-quick-add:hover:not(:disabled) {
  background: var(--ws-primary);
  color: #fff;
}
.ws-quick-add:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}
.ws-product-body {
  padding: 0.9rem 1rem 1.1rem;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1 1 auto;
}
.ws-product-brand {
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--ws-muted);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.ws-product-title {
  font-family: var(--ws-font);
  font-size: 0.95rem;
  font-weight: 600;
  margin: 0;
  color: var(--ws-ink);
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  min-height: 2.5em;
}
</style>
