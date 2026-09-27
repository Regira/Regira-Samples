<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink, useRouter } from "vue-router"
import { LoadingContainer, useAppFeedback } from "@regira/modules/vue/ui"
import { useEntityStore as useProductStore, type Entity as Product } from "@/entities/products"
import { useCartStore } from "../cart"
import { money } from "../money"
import ProductVisual from "../components/ProductVisual.vue"
import RatingStars from "../components/RatingStars.vue"
import QuantityStepper from "../components/QuantityStepper.vue"
import ProductRail from "../components/ProductRail.vue"

const props = defineProps<{ id: string }>()
const { service, fromPool } = useProductStore()
const cart = useCartStore()
const feedback = useAppFeedback()
const router = useRouter()

const product = ref<Product>()
const isLoading = ref(true)
const notFound = ref(false)
const quantity = ref(1)
const tab = ref<"details" | "shipping">("details")

async function load() {
    isLoading.value = true
    notFound.value = false
    quantity.value = 1
    try {
        const p = await service.details(props.id)
        product.value = p ? fromPool(p) : undefined
        notFound.value = !p
    } catch (ex) {
        console.error("loading product failed", ex)
        notFound.value = true
    } finally {
        isLoading.value = false
    }
}
watch(() => props.id, load, { immediate: true })

const inCart = computed(() => (product.value ? cart.quantityOf(product.value.id) : 0))
const available = computed(() => (product.value ? Math.max(0, product.value.stock - inCart.value) : 0))
const soldOut = computed(() => !product.value || product.value.stock <= 0 || !product.value.isActive)
// the description's first sentence lists the product's highlights ("features a, b and c.")
const highlights = computed(() => {
    const match = product.value?.description?.match(/features (.+?)\. /)
    return match ? match[1]!.split(/, | and /).map((x) => x.charAt(0).toUpperCase() + x.slice(1)) : []
})

function addToCart(goToCart = false) {
    if (!product.value) return
    cart.add(product.value, quantity.value)
    if (goToCart) router.push({ name: "cart" })
    else feedback.success(`Added ${quantity.value} x "${product.value.title}" to your cart`)
    quantity.value = 1
}
</script>

<template>
    <div class="container-xl py-3 py-md-4">
        <LoadingContainer :is-loading="isLoading">
            <div v-if="notFound" class="ws-empty">
                <i class="bi bi-emoji-frown"></i>
                <h5>We couldn't find that product</h5>
                <RouterLink :to="{ name: 'shop' }" class="btn btn-primary">Back to the shop</RouterLink>
            </div>
            <template v-else-if="product">
                <nav aria-label="breadcrumb">
                    <ol class="breadcrumb small">
                        <li class="breadcrumb-item"><RouterLink :to="{ name: 'home' }">Home</RouterLink></li>
                        <li class="breadcrumb-item"><RouterLink :to="{ name: 'shop' }">Shop</RouterLink></li>
                        <li v-if="product.category" class="breadcrumb-item">
                            <RouterLink :to="{ name: 'shop', query: { categoryId: product.categoryId } }">{{ product.category.title }}</RouterLink>
                        </li>
                        <li class="breadcrumb-item active text-truncate">{{ product.title }}</li>
                    </ol>
                </nav>

                <div class="row g-4 g-lg-5">
                    <div class="col-md-6">
                        <div class="ws-pdp-media">
                            <ProductVisual :seed="product.id" :icon="product.category?.icon" :color="product.category?.color" :image-url="product.imageUrl" :label="product.brand?.title" size="lg" />
                            <div class="ws-card__badges">
                                <span v-if="product.$isOnSale" class="badge ws-badge-sale">-{{ product.$discountPercentage }}%</span>
                                <span v-if="product.$isNew" class="badge ws-badge-new">New</span>
                                <span v-if="product.isFeatured" class="badge ws-badge-featured"><i class="bi bi-stars"></i> Staff pick</span>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-6">
                        <div class="ws-pdp-brand">{{ product.brand?.title }}</div>
                        <h1 class="ws-pdp-title">{{ product.title }}</h1>
                        <div class="d-flex flex-wrap align-items-center gap-3 mb-3">
                            <RatingStars :rating="product.rating" :count="product.reviewCount" />
                            <span class="text-body-secondary small">SKU {{ product.sku }}</span>
                        </div>

                        <div class="ws-price ws-price--lg mb-2">
                            <span class="ws-price__now" :class="{ 'is-sale': product.$isOnSale }">{{ money(product.price) }}</span>
                            <s v-if="product.$isOnSale" class="ws-price__was">{{ money(product.compareAtPrice) }}</s>
                            <span v-if="product.$isOnSale" class="ws-save">You save {{ money(product.compareAtPrice! - product.price) }}</span>
                        </div>
                        <p class="small text-body-secondary mb-4">Incl. VAT. {{ product.price >= 50 ? "Free standard shipping." : `Free shipping from ${money(50)}.` }}</p>

                        <div class="mb-3">
                            <span v-if="!product.isActive" class="ws-stock ws-stock--out"><i class="bi bi-x-circle"></i> No longer available</span>
                            <span v-else-if="product.stock <= 0" class="ws-stock ws-stock--out"><i class="bi bi-x-circle"></i> Sold out</span>
                            <span v-else-if="product.stock <= 5" class="ws-stock ws-stock--low"><i class="bi bi-exclamation-circle"></i> Only {{ product.stock }} left - order soon</span>
                            <span v-else class="ws-stock ws-stock--in"><i class="bi bi-check-circle"></i> In stock, ships within 24 hours</span>
                        </div>

                        <div class="d-flex flex-wrap gap-2 mb-2">
                            <QuantityStepper v-model="quantity" :max="Math.max(1, available)" class="ws-stepper--pdp" />
                            <button type="button" class="btn btn-primary btn-lg flex-grow-1" :disabled="soldOut || available <= 0" @click="addToCart(false)">
                                <i class="bi bi-cart-plus me-2"></i>Add to cart
                            </button>
                            <button type="button" class="btn btn-dark btn-lg" :disabled="soldOut || available <= 0" @click="addToCart(true)">Buy now</button>
                        </div>
                        <p v-if="inCart" class="small text-success mb-4">
                            <i class="bi bi-bag-check me-1"></i>{{ inCart }} already in your cart - <RouterLink :to="{ name: 'cart' }">view cart</RouterLink>
                        </p>

                        <ul v-if="highlights.length" class="ws-highlights">
                            <li v-for="h in highlights" :key="h"><i class="bi bi-check2"></i>{{ h }}</li>
                        </ul>

                        <div class="ws-perks">
                            <div><i class="bi bi-truck"></i>Delivery in 2-4 days, next-day with Express</div>
                            <div><i class="bi bi-arrow-repeat"></i>30-day free returns</div>
                            <div><i class="bi bi-shield-check"></i>2-year warranty</div>
                        </div>
                    </div>
                </div>

                <div class="ws-pdp-tabs mt-5">
                    <ul class="nav nav-underline mb-3">
                        <li class="nav-item"><button type="button" class="nav-link" :class="{ active: tab === 'details' }" @click="tab = 'details'">Description</button></li>
                        <li class="nav-item"><button type="button" class="nav-link" :class="{ active: tab === 'shipping' }" @click="tab = 'shipping'">Shipping &amp; returns</button></li>
                    </ul>
                    <div v-if="tab === 'details'" class="ws-prose">
                        <p>{{ product.description }}</p>
                        <dl class="row small mb-0">
                            <dt class="col-sm-3">Brand</dt>
                            <dd class="col-sm-9">{{ product.brand?.title }}</dd>
                            <dt class="col-sm-3">Category</dt>
                            <dd class="col-sm-9">{{ product.category?.title }}</dd>
                            <dt class="col-sm-3">SKU</dt>
                            <dd class="col-sm-9">{{ product.sku }}</dd>
                        </dl>
                    </div>
                    <div v-else class="ws-prose">
                        <p>
                            Standard delivery takes 2-4 business days and is free on orders over {{ money(50) }} ({{ money(4.95) }} otherwise). Express
                            delivery arrives the next business day for {{ money(9.95) }}, or pick your order up in our Brussels store for free.
                        </p>
                        <p class="mb-0">Changed your mind? Return any unused product within 30 days for a full refund.</p>
                    </div>
                </div>

                <ProductRail
                    v-if="product.categoryId"
                    class="mt-5"
                    title="You might also like"
                    :query="{ categoryId: product.categoryId, exclude: [product.id], inStock: true, sortBy: ['Popularity'] }"
                    :more-link="{ name: 'shop', query: { categoryId: product.categoryId } }"
                    :size="8"
                />
            </template>
        </LoadingContainer>
    </div>
</template>
