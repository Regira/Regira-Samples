<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { useEntityStore as useProductStore } from "@/entities/products"
import { useCartStore, MAX_QUANTITY } from "../cart"
import { money, FREE_SHIPPING_THRESHOLD } from "../money"
import ProductVisual from "../components/ProductVisual.vue"
import QuantityStepper from "../components/QuantityStepper.vue"
import ProductRail from "../components/ProductRail.vue"

const cart = useCartStore()
const { service } = useProductStore()
const refreshing = ref(false)

// prices and stock may have changed since the product was added: refresh the snapshots
onMounted(async () => {
    if (!cart.lines.length) return
    refreshing.value = true
    try {
        const { items } = await service.search({ ids: cart.lines.map((l) => l.productId), pageSize: 0 })
        cart.refresh(items)
    } catch (ex) {
        console.error("refreshing the cart failed", ex)
    } finally {
        refreshing.value = false
    }
})
const progress = computed(() => Math.min(100, Math.round((cart.subtotal / FREE_SHIPPING_THRESHOLD) * 100)))
</script>

<template>
    <div class="container-xl py-3 py-md-4">
        <h1 class="ws-page-title mb-4">Your cart <small v-if="cart.count" class="text-body-secondary fs-6 fw-normal">({{ cart.count }} items)</small></h1>

        <div v-if="!cart.lines.length" class="ws-empty">
            <i class="bi bi-bag"></i>
            <h5>Your cart is empty</h5>
            <p class="text-body-secondary">Looks like you haven't added anything yet.</p>
            <RouterLink :to="{ name: 'shop' }" class="btn btn-primary">Start shopping</RouterLink>
        </div>

        <div v-else class="row g-4">
            <div class="col-lg-8">
                <div class="ws-freeship mb-3">
                    <template v-if="cart.freeShippingRemaining > 0">
                        <i class="bi bi-truck me-2"></i>Add <strong>{{ money(cart.freeShippingRemaining) }}</strong> more for free standard shipping
                    </template>
                    <template v-else><i class="bi bi-check-circle-fill text-success me-2"></i>You qualify for <strong>free standard shipping</strong></template>
                    <div class="progress mt-2" role="progressbar" :aria-valuenow="progress" aria-valuemin="0" aria-valuemax="100" style="height: 6px">
                        <div class="progress-bar" :style="{ width: progress + '%' }"></div>
                    </div>
                </div>

                <div class="ws-panel">
                    <div v-for="line in cart.lines" :key="line.productId" class="ws-cart-line">
                        <RouterLink :to="{ name: 'product', params: { id: line.productId } }" class="ws-cart-line__media">
                            <ProductVisual :seed="line.productId" :icon="line.product.categoryIcon" :color="line.product.categoryColor" :image-url="line.product.imageUrl" size="sm" />
                        </RouterLink>
                        <div class="ws-cart-line__info">
                            <small class="text-body-secondary">{{ line.product.brandTitle }}</small>
                            <RouterLink :to="{ name: 'product', params: { id: line.productId } }" class="ws-cart-line__title">{{ line.product.title }}</RouterLink>
                            <div class="small">
                                {{ money(line.product.price) }}
                                <s v-if="line.product.compareAtPrice && line.product.compareAtPrice > line.product.price" class="text-body-secondary ms-1">{{ money(line.product.compareAtPrice) }}</s>
                            </div>
                            <div v-if="!line.product.isActive || line.product.stock <= 0" class="small text-danger"><i class="bi bi-x-circle me-1"></i>No longer available - please remove it</div>
                            <div v-else-if="line.product.stock < line.quantity" class="small text-danger">
                                <i class="bi bi-exclamation-circle me-1"></i>Only {{ line.product.stock }} left in stock
                            </div>
                        </div>
                        <div class="ws-cart-line__qty">
                            <QuantityStepper
                                :model-value="line.quantity"
                                :max="Math.max(1, Math.min(MAX_QUANTITY, line.product.stock))"
                                size="sm"
                                @update:model-value="cart.setQuantity(line.productId, $event)"
                            />
                        </div>
                        <div class="ws-cart-line__total">{{ money(line.product.price * line.quantity) }}</div>
                        <button type="button" class="btn btn-link text-body-secondary ws-cart-line__remove" title="Remove" @click="cart.remove(line.productId)">
                            <i class="bi bi-trash3"></i>
                        </button>
                    </div>
                </div>
                <div class="d-flex justify-content-between mt-3">
                    <RouterLink :to="{ name: 'shop' }" class="btn btn-link px-0"><i class="bi bi-arrow-left me-1"></i>Continue shopping</RouterLink>
                    <button type="button" class="btn btn-link text-danger px-0" @click="cart.clear()">Empty cart</button>
                </div>
            </div>

            <div class="col-lg-4">
                <div class="ws-panel ws-summary">
                    <h5 class="mb-3">Order summary</h5>
                    <div class="ws-summary__row"><span>Subtotal</span><span>{{ money(cart.subtotal) }}</span></div>
                    <div v-if="cart.savings > 0" class="ws-summary__row text-success"><span>You save</span><span>-{{ money(cart.savings) }}</span></div>
                    <div class="ws-summary__row">
                        <span>Shipping</span><span>{{ cart.freeShippingRemaining > 0 ? `from ${money(4.95)}` : "Free" }}</span>
                    </div>
                    <div class="ws-summary__row ws-summary__total"><span>Estimated total</span><span>{{ money(cart.subtotal + (cart.freeShippingRemaining > 0 ? 4.95 : 0)) }}</span></div>
                    <RouterLink
                        :to="{ name: 'checkout' }"
                        class="btn btn-primary btn-lg w-100 mt-3"
                        :class="{ disabled: refreshing || cart.unavailableLines.length > 0 }"
                    >
                        <i class="bi bi-lock me-2"></i>Checkout
                    </RouterLink>
                    <p v-if="cart.unavailableLines.length" class="small text-danger mt-2 mb-0">Some items are unavailable in the requested quantity.</p>
                    <div class="ws-paylogos ws-paylogos--dark mt-3">
                        <span><i class="bi bi-credit-card"></i></span><span><i class="bi bi-paypal"></i></span><span><i class="bi bi-bank"></i></span><span><i class="bi bi-cash-coin"></i></span>
                    </div>
                </div>
            </div>
        </div>

        <ProductRail v-if="cart.lines.length" class="mt-5" title="Deals you might like" :query="{ onSale: true, inStock: true, sortBy: ['Popularity'] }" :size="8" />
    </div>
</template>
