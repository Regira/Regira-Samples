<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import { useAppFeedback } from "@regira/modules/vue/ui"
import type { Entity as Product } from "@/entities/products"
import { useCartStore } from "../cart"
import { money } from "../money"
import ProductVisual from "./ProductVisual.vue"
import RatingStars from "./RatingStars.vue"

const props = defineProps<{ product: Product }>()
const cart = useCartStore()
const feedback = useAppFeedback()
const inCart = computed(() => cart.quantityOf(props.product.id))
const soldOut = computed(() => props.product.stock <= 0)
const canAdd = computed(() => !soldOut.value && inCart.value < props.product.stock)

function addToCart() {
    cart.add(props.product, 1)
    feedback.success(`Added "${props.product.title}" to your cart`)
}
</script>

<template>
    <article class="ws-card h-100">
        <RouterLink :to="{ name: 'product', params: { id: product.id } }" class="ws-card__media">
            <ProductVisual :seed="product.id" :icon="product.category?.icon" :color="product.category?.color" :image-url="product.imageUrl" :label="product.brand?.title" />
            <div class="ws-card__badges">
                <span v-if="product.$isOnSale" class="badge ws-badge-sale">-{{ product.$discountPercentage }}%</span>
                <span v-if="product.$isNew" class="badge ws-badge-new">New</span>
                <span v-if="product.isFeatured" class="badge ws-badge-featured"><i class="bi bi-stars"></i> Pick</span>
            </div>
            <span v-if="soldOut" class="ws-card__soldout">Sold out</span>
        </RouterLink>
        <div class="ws-card__body">
            <div class="ws-card__brand">{{ product.brand?.title }}</div>
            <RouterLink :to="{ name: 'product', params: { id: product.id } }" class="ws-card__title">{{ product.title }}</RouterLink>
            <RatingStars :rating="product.rating" :count="product.reviewCount" small class="mb-2" />
            <div class="ws-card__footer">
                <div class="ws-price">
                    <span class="ws-price__now" :class="{ 'is-sale': product.$isOnSale }">{{ money(product.price) }}</span>
                    <s v-if="product.$isOnSale" class="ws-price__was">{{ money(product.compareAtPrice) }}</s>
                </div>
                <button
                    type="button"
                    class="btn btn-sm ws-btn-cart"
                    :class="inCart ? 'btn-success' : 'btn-primary'"
                    :disabled="!canAdd"
                    :title="soldOut ? 'Sold out' : 'Add to cart'"
                    @click="addToCart"
                >
                    <i class="bi" :class="inCart ? 'bi-cart-check' : 'bi-cart-plus'"></i>
                    <span v-if="inCart" class="ms-1">{{ inCart }}</span>
                </button>
            </div>
            <small v-if="!soldOut && product.stock <= 5" class="text-danger-emphasis mt-1 d-block">Only {{ product.stock }} left</small>
        </div>
    </article>
</template>
