<script setup lang="ts">
import { useCartStore } from "@/stores/cart"

const cart = useCartStore()

function fmt(v: number) {
  return new Intl.NumberFormat("en-IE", { style: "currency", currency: "EUR" }).format(v)
}

const FREE_SHIPPING_THRESHOLD = 75
const SHIPPING_FLAT = 5.95
</script>

<template>
  <div class="ws-container ws-cart-page">
    <h1 class="display-font">Your cart</h1>

    <div v-if="cart.isEmpty" class="ws-empty">
      <i class="bi bi-bag"></i>
      <h3>Your cart is empty</h3>
      <p class="ws-muted">Looks like you haven't added anything yet.</p>
      <RouterLink :to="{ name: 'shop' }" class="btn-ws btn-ws-primary mt-2">Start shopping</RouterLink>
    </div>

    <div v-else class="ws-cart-layout">
      <div class="ws-cart-lines">
        <div v-for="line in cart.lines" :key="line.productId" class="ws-cart-line">
          <RouterLink :to="{ name: 'product-detail', params: { slug: line.slug ?? String(line.productId) } }" class="ws-cart-line-img">
            <img :src="line.imageUrl ?? ''" :alt="line.title" />
          </RouterLink>
          <div class="ws-cart-line-body">
            <RouterLink :to="{ name: 'product-detail', params: { slug: line.slug ?? String(line.productId) } }" class="ws-cart-line-title">
              {{ line.title }}
            </RouterLink>
            <span class="ws-muted">{{ fmt(line.price) }} each</span>
            <span v-if="line.quantity >= line.stock" class="ws-low-note">Max available quantity reached</span>
          </div>
          <div class="ws-qty ws-qty-sm">
            <button type="button" @click="cart.setQuantity(line.productId, line.quantity - 1)"><i class="bi bi-dash"></i></button>
            <input
              type="number"
              :value="line.quantity"
              min="1"
              :max="line.stock"
              @change="cart.setQuantity(line.productId, Number(($event.target as HTMLInputElement).value))"
            />
            <button type="button" @click="cart.setQuantity(line.productId, line.quantity + 1)"><i class="bi bi-plus"></i></button>
          </div>
          <div class="ws-cart-line-total">{{ fmt(line.price * line.quantity) }}</div>
          <button type="button" class="ws-remove-btn" aria-label="Remove" @click="cart.remove(line.productId)">
            <i class="bi bi-trash3"></i>
          </button>
        </div>
      </div>

      <aside class="ws-cart-summary">
        <h6>Order summary</h6>
        <div class="ws-summary-row">
          <span>Subtotal</span>
          <span>{{ fmt(cart.subTotal) }}</span>
        </div>
        <div class="ws-summary-row">
          <span>Shipping</span>
          <span>{{ cart.subTotal >= FREE_SHIPPING_THRESHOLD ? "Free" : fmt(SHIPPING_FLAT) }}</span>
        </div>
        <p v-if="cart.subTotal < FREE_SHIPPING_THRESHOLD" class="ws-shipping-hint">
          Add {{ fmt(FREE_SHIPPING_THRESHOLD - cart.subTotal) }} more for free shipping
        </p>
        <div class="ws-summary-row ws-summary-total">
          <span>Total</span>
          <span>{{ fmt(cart.subTotal + (cart.subTotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_FLAT)) }}</span>
        </div>
        <RouterLink :to="{ name: 'checkout' }" class="btn-ws btn-ws-primary btn-ws-block">
          Proceed to checkout <i class="bi bi-arrow-right"></i>
        </RouterLink>
        <RouterLink :to="{ name: 'shop' }" class="ws-continue-link">Continue shopping</RouterLink>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.ws-cart-page {
  padding: 2.5rem 1.5rem 5rem;
}
.ws-cart-page h1 {
  font-size: 1.9rem;
  margin-bottom: 1.6rem;
}
.ws-cart-layout {
  display: grid;
  grid-template-columns: 1fr 320px;
  gap: 2.5rem;
  align-items: start;
}
@media (max-width: 850px) {
  .ws-cart-layout {
    grid-template-columns: 1fr;
  }
}
.ws-cart-lines {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.ws-cart-line {
  display: grid;
  grid-template-columns: 80px 1fr auto auto auto;
  align-items: center;
  gap: 1rem;
  background: var(--ws-surface);
  border: 1px solid var(--ws-line);
  border-radius: var(--ws-radius);
  padding: 0.8rem;
}
@media (max-width: 560px) {
  .ws-cart-line {
    grid-template-columns: 60px 1fr;
    grid-template-areas: "img body" "img qty" "img total";
  }
  .ws-remove-btn {
    grid-area: auto;
  }
}
.ws-cart-line-img {
  width: 80px;
  height: 80px;
  border-radius: var(--ws-radius-sm);
  overflow: hidden;
  background: var(--ws-bg-alt);
}
.ws-cart-line-img img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
.ws-cart-line-body {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
  font-size: 0.9rem;
  min-width: 0;
}
.ws-cart-line-title {
  font-weight: 600;
  color: var(--ws-ink);
}
.ws-low-note {
  color: #8a5a00;
  font-size: 0.76rem;
}
.ws-qty {
  display: flex;
  align-items: center;
  border: 1px solid var(--ws-line);
  border-radius: 999px;
  overflow: hidden;
}
.ws-qty-sm button {
  width: 2rem;
  height: 2.1rem;
  border: none;
  background: var(--ws-bg-alt);
  cursor: pointer;
}
.ws-qty-sm input {
  width: 2.4rem;
  text-align: center;
  border: none;
  font-weight: 700;
}
.ws-cart-line-total {
  font-weight: 700;
  min-width: 4.5rem;
  text-align: right;
}
.ws-remove-btn {
  border: none;
  background: transparent;
  color: var(--ws-muted);
  cursor: pointer;
  font-size: 1.1rem;
}
.ws-remove-btn:hover {
  color: var(--ws-sale);
}
.ws-cart-summary {
  background: var(--ws-surface);
  border: 1px solid var(--ws-line);
  border-radius: var(--ws-radius);
  padding: 1.5rem;
  position: sticky;
  top: 6rem;
}
.ws-cart-summary h6 {
  font-weight: 800;
  margin-bottom: 1rem;
}
.ws-summary-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
  padding: 0.35rem 0;
  color: var(--ws-ink-soft);
}
.ws-summary-total {
  font-weight: 800;
  color: var(--ws-ink);
  font-size: 1.05rem;
  border-top: 1px solid var(--ws-line);
  margin-top: 0.5rem;
  padding-top: 0.8rem;
}
.ws-shipping-hint {
  font-size: 0.76rem;
  color: var(--ws-primary);
  margin: 0 0 0.4rem;
}
.ws-cart-summary .btn-ws {
  margin-top: 1.1rem;
}
.ws-continue-link {
  display: block;
  text-align: center;
  margin-top: 0.8rem;
  font-size: 0.85rem;
  color: var(--ws-muted);
}
.ws-continue-link:hover {
  color: var(--ws-primary);
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
