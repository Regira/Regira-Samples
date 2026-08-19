<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import type { ProductDto } from "@/types/models"
import { fetchProduct, fetchProductBySlug, fetchRelatedProducts } from "@/api/products"
import RatingStars from "@/components/product/RatingStars.vue"
import PriceTag from "@/components/product/PriceTag.vue"
import ProductCard from "@/components/product/ProductCard.vue"
import { useCartStore } from "@/stores/cart"
import { useToastStore } from "@/stores/toast"

const route = useRoute()
const router = useRouter()
const cart = useCartStore()
const toast = useToastStore()

const product = ref<ProductDto | null>(null)
const related = ref<ProductDto[]>([])
const loading = ref(true)
const quantity = ref(1)
const notFound = ref(false)

const outOfStock = computed(() => (product.value ? product.value.stock <= 0 : false))

async function load(slug: string) {
  loading.value = true
  notFound.value = false
  quantity.value = 1
  try {
    const found = await resolveBySlug(slug)
    if (!found) {
      notFound.value = true
      return
    }
    product.value = found
    fetchRelatedProducts(found.categoryId, found.id, 4).then((items) => (related.value = items))
  } finally {
    loading.value = false
  }
}

async function resolveBySlug(slug: string): Promise<ProductDto | null> {
  if (/^\d+$/.test(slug)) {
    try {
      return await fetchProduct(Number(slug))
    } catch {
      return null
    }
  }
  return fetchProductBySlug(slug)
}

function addToCart() {
  if (!product.value || outOfStock.value) return
  cart.add(product.value, quantity.value)
  toast.success(`Added ${quantity.value} x "${product.value.title}" to cart`)
}

function buyNow() {
  addToCart()
  router.push({ name: "cart" })
}

watch(() => route.params.slug, (slug) => load(slug as string), { immediate: true })
</script>

<template>
  <div class="ws-container ws-detail">
    <div v-if="loading" class="ws-detail-grid">
      <div class="ws-skeleton" style="aspect-ratio: 1/1"></div>
      <div>
        <div class="ws-skeleton" style="height: 2rem; width: 70%; margin-bottom: 1rem"></div>
        <div class="ws-skeleton" style="height: 1rem; width: 40%; margin-bottom: 2rem"></div>
        <div class="ws-skeleton" style="height: 8rem"></div>
      </div>
    </div>

    <div v-else-if="notFound || !product" class="ws-empty">
      <i class="bi bi-search"></i>
      <h3>Product not found</h3>
      <RouterLink :to="{ name: 'shop' }" class="btn-ws btn-ws-primary mt-2">Back to shop</RouterLink>
    </div>

    <template v-else>
      <nav class="ws-breadcrumb">
        <RouterLink :to="{ name: 'home' }">Home</RouterLink>
        <i class="bi bi-chevron-right"></i>
        <RouterLink :to="{ name: 'shop' }">Shop</RouterLink>
        <template v-if="product.category">
          <i class="bi bi-chevron-right"></i>
          <RouterLink :to="{ name: 'shop-category', params: { categorySlug: product.category.slug ?? '' } }">{{ product.category.title }}</RouterLink>
        </template>
        <i class="bi bi-chevron-right"></i>
        <span class="ws-muted">{{ product.title }}</span>
      </nav>

      <div class="ws-detail-grid">
        <div class="ws-detail-media">
          <img :src="product.imageUrl ?? ''" :alt="product.title" />
          <span v-if="product.compareAtPrice && product.compareAtPrice > product.price" class="ws-badge ws-badge-sale ws-detail-badge">Sale</span>
        </div>

        <div class="ws-detail-info">
          <span class="ws-product-brand">{{ product.brand }}</span>
          <h1 class="display-font">{{ product.title }}</h1>
          <RatingStars :rating="product.rating" :review-count="product.reviewCount" />
          <PriceTag :price="product.price" :compare-at-price="product.compareAtPrice" size="lg" class="my-3" />
          <p class="ws-detail-desc">{{ product.description }}</p>

          <ul class="ws-detail-meta">
            <li><strong>SKU</strong> {{ product.code }}</li>
            <li>
              <strong>Availability</strong>
              <span v-if="outOfStock" class="ws-out-text">Out of stock</span>
              <span v-else-if="product.stock <= 5" class="ws-low-text">Only {{ product.stock }} left in stock</span>
              <span v-else class="ws-in-text">In stock</span>
            </li>
          </ul>

          <div class="ws-detail-actions">
            <div class="ws-qty" v-if="!outOfStock">
              <button type="button" @click="quantity = Math.max(1, quantity - 1)"><i class="bi bi-dash"></i></button>
              <input type="number" v-model.number="quantity" min="1" :max="product.stock" />
              <button type="button" @click="quantity = Math.min(product.stock, quantity + 1)"><i class="bi bi-plus"></i></button>
            </div>
            <button type="button" class="btn-ws btn-ws-outline" :disabled="outOfStock" @click="addToCart">
              <i class="bi bi-bag-plus"></i> Add to cart
            </button>
            <button type="button" class="btn-ws btn-ws-primary" :disabled="outOfStock" @click="buyNow">Buy now</button>
          </div>
        </div>
      </div>

      <section v-if="related.length" class="ws-related">
        <h2 class="display-font">You might also like</h2>
        <div class="ws-product-grid">
          <ProductCard v-for="p in related" :key="p.id" :product="p" />
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.ws-detail {
  padding: 2rem 1.5rem 4rem;
}
.ws-breadcrumb {
  display: flex;
  align-items: center;
  gap: 0.4rem;
  font-size: 0.82rem;
  color: var(--ws-muted);
  margin-bottom: 1.4rem;
  flex-wrap: wrap;
}
.ws-breadcrumb a:hover {
  color: var(--ws-primary);
}
.ws-detail-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 3rem;
}
@media (max-width: 850px) {
  .ws-detail-grid {
    grid-template-columns: 1fr;
    gap: 1.5rem;
  }
}
.ws-detail-media {
  position: relative;
  border-radius: var(--ws-radius-lg);
  overflow: hidden;
  background: var(--ws-bg-alt);
  aspect-ratio: 1/1;
}
.ws-detail-media img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.ws-detail-badge {
  position: absolute;
  top: 1rem;
  left: 1rem;
}
.ws-product-brand {
  font-size: 0.78rem;
  font-weight: 700;
  color: var(--ws-muted);
  text-transform: uppercase;
  letter-spacing: 0.03em;
}
.ws-detail-info h1 {
  font-size: 1.9rem;
  margin: 0.4rem 0 0.6rem;
}
.ws-detail-desc {
  color: var(--ws-ink-soft);
  line-height: 1.6;
}
.ws-detail-meta {
  list-style: none;
  padding: 0;
  margin: 1.2rem 0;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  font-size: 0.88rem;
  border-top: 1px solid var(--ws-line);
  padding-top: 1.2rem;
}
.ws-detail-meta strong {
  display: inline-block;
  width: 8rem;
  color: var(--ws-ink-soft);
}
.ws-out-text {
  color: var(--ws-sale);
  font-weight: 700;
}
.ws-low-text {
  color: #8a5a00;
  font-weight: 700;
}
.ws-in-text {
  color: var(--ws-success);
  font-weight: 700;
}
.ws-detail-actions {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  flex-wrap: wrap;
}
.ws-qty {
  display: flex;
  align-items: center;
  border: 1px solid var(--ws-line);
  border-radius: 999px;
  overflow: hidden;
}
.ws-qty button {
  border: none;
  background: var(--ws-bg-alt);
  width: 2.4rem;
  height: 2.6rem;
  cursor: pointer;
  font-size: 1rem;
}
.ws-qty input {
  width: 3rem;
  text-align: center;
  border: none;
  font-weight: 700;
  -moz-appearance: textfield;
}
.ws-qty input::-webkit-outer-spin-button,
.ws-qty input::-webkit-inner-spin-button {
  -webkit-appearance: none;
}
.ws-related {
  margin-top: 4rem;
}
.ws-related h2 {
  font-size: 1.4rem;
  margin-bottom: 1.4rem;
}
.ws-product-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 1.25rem;
}
@media (max-width: 900px) {
  .ws-product-grid {
    grid-template-columns: repeat(2, 1fr);
  }
}
.ws-empty {
  text-align: center;
  padding: 5rem 1rem;
}
.ws-empty i {
  font-size: 2.4rem;
  color: var(--ws-muted);
}
</style>
