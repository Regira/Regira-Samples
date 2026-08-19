<script setup lang="ts">
import { onMounted, ref } from "vue"
import { useRoute } from "vue-router"
import type { OrderDto } from "@/types/models"
import { fetchOrder } from "@/api/orders"

const route = useRoute()
const order = ref<OrderDto | null>(null)
const loading = ref(true)
const failed = ref(false)

function fmt(v: number) {
  return new Intl.NumberFormat("en-IE", { style: "currency", currency: "EUR" }).format(v)
}

onMounted(async () => {
  try {
    order.value = await fetchOrder(Number(route.params.id))
  } catch {
    failed.value = true
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="ws-container ws-confirm">
    <div v-if="loading" class="ws-confirm-loading">
      <i class="bi bi-arrow-repeat spin"></i>
    </div>
    <div v-else-if="failed || !order" class="ws-empty">
      <i class="bi bi-exclamation-triangle"></i>
      <h3>We couldn't find that order</h3>
      <RouterLink :to="{ name: 'shop' }" class="btn-ws btn-ws-primary mt-2">Back to shop</RouterLink>
    </div>
    <template v-else>
      <div class="ws-confirm-head">
        <div class="ws-confirm-icon"><i class="bi bi-check-lg"></i></div>
        <h1 class="display-font">Thank you, {{ order.customerName.split(" ")[0] }}!</h1>
        <p class="ws-muted">Your order has been placed successfully. A confirmation has been sent to {{ order.customerEmail }}.</p>
        <div class="ws-confirm-code">Order {{ order.code }}</div>
      </div>

      <div class="ws-confirm-grid">
        <div class="ws-card ws-confirm-lines">
          <h6>Items</h6>
          <div v-for="line in order.orderLines" :key="line.id" class="ws-confirm-line">
            <img :src="line.product?.imageUrl ?? ''" :alt="line.product?.title" />
            <div>
              <span class="ws-summary-line-title">{{ line.product?.title }}</span>
              <span class="ws-muted">Qty {{ line.quantity }} &times; {{ fmt(line.unitPrice) }}</span>
            </div>
            <span class="ws-summary-line-price">{{ fmt(line.subTotal) }}</span>
          </div>
          <div class="ws-confirm-total">
            <span>Total</span>
            <span>{{ fmt(order.total) }}</span>
          </div>
        </div>

        <div class="ws-card ws-confirm-address">
          <h6>Shipping to</h6>
          <p>
            {{ order.customerName }}<br />
            {{ order.shippingAddress }}<br />
            {{ order.shippingPostalCode }} {{ order.shippingCity }}<br />
            {{ order.shippingCountry }}
          </p>
          <h6 class="mt-3">Status</h6>
          <span class="ws-status-pill">{{ order.status }}</span>
        </div>
      </div>

      <div class="ws-confirm-actions">
        <RouterLink :to="{ name: 'shop' }" class="btn-ws btn-ws-primary">Continue shopping</RouterLink>
      </div>
    </template>
  </div>
</template>

<style scoped>
.ws-confirm {
  padding: 3rem 1.5rem 5rem;
  max-width: 900px;
}
.ws-confirm-loading {
  text-align: center;
  padding: 5rem 0;
  font-size: 2rem;
  color: var(--ws-primary);
}
.spin {
  display: inline-block;
  animation: ws-spin 0.9s linear infinite;
}
@keyframes ws-spin {
  to {
    transform: rotate(360deg);
  }
}
.ws-confirm-head {
  text-align: center;
  margin-bottom: 2.5rem;
}
.ws-confirm-icon {
  width: 4rem;
  height: 4rem;
  border-radius: 50%;
  background: var(--ws-success);
  color: #fff;
  font-size: 1.8rem;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 1rem;
}
.ws-confirm-head h1 {
  font-size: 1.8rem;
  margin: 0 0 0.5rem;
}
.ws-confirm-code {
  display: inline-block;
  margin-top: 0.8rem;
  background: var(--ws-bg-alt);
  color: var(--ws-primary);
  font-weight: 700;
  padding: 0.4rem 1rem;
  border-radius: 999px;
  font-size: 0.85rem;
}
.ws-confirm-grid {
  display: grid;
  grid-template-columns: 1.5fr 1fr;
  gap: 1.5rem;
}
@media (max-width: 700px) {
  .ws-confirm-grid {
    grid-template-columns: 1fr;
  }
}
.ws-confirm-lines,
.ws-confirm-address {
  padding: 1.4rem 1.5rem;
}
.ws-confirm-lines h6,
.ws-confirm-address h6 {
  font-weight: 800;
  margin-bottom: 1rem;
}
.ws-confirm-line {
  display: flex;
  align-items: center;
  gap: 0.8rem;
  padding: 0.6rem 0;
  border-bottom: 1px solid var(--ws-line);
}
.ws-confirm-line img {
  width: 48px;
  height: 48px;
  border-radius: var(--ws-radius-sm);
  object-fit: cover;
  background: var(--ws-bg-alt);
}
.ws-confirm-line div {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  font-size: 0.85rem;
}
.ws-summary-line-title {
  font-weight: 600;
}
.ws-summary-line-price {
  font-weight: 700;
}
.ws-confirm-total {
  display: flex;
  justify-content: space-between;
  font-weight: 800;
  font-size: 1.05rem;
  padding-top: 1rem;
}
.ws-confirm-address p {
  font-size: 0.9rem;
  line-height: 1.6;
  color: var(--ws-ink-soft);
}
.ws-status-pill {
  display: inline-block;
  background: #eef2ff;
  color: var(--ws-primary);
  font-weight: 700;
  font-size: 0.78rem;
  padding: 0.3rem 0.75rem;
  border-radius: 999px;
}
.ws-confirm-actions {
  text-align: center;
  margin-top: 2.5rem;
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
