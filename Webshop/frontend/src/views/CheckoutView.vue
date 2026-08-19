<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { useRouter } from "vue-router"
import { useCartStore } from "@/stores/cart"
import { useToastStore } from "@/stores/toast"
import { createOrder, OrderValidationError } from "@/api/orders"
import type { FieldErrors, OrderInputDto } from "@/types/models"

const cart = useCartStore()
const toast = useToastStore()
const router = useRouter()

const form = reactive({
  customerName: "",
  customerEmail: "",
  customerPhone: "",
  shippingAddress: "",
  shippingCity: "",
  shippingPostalCode: "",
  shippingCountry: "Belgium",
})

const errors = ref<FieldErrors>({})
const submitting = ref(false)

const FREE_SHIPPING_THRESHOLD = 75
const SHIPPING_FLAT = 5.95
const shippingCost = computed(() => (cart.subTotal >= FREE_SHIPPING_THRESHOLD ? 0 : SHIPPING_FLAT))
const total = computed(() => cart.subTotal + shippingCost.value)

function fmt(v: number) {
  return new Intl.NumberFormat("en-IE", { style: "currency", currency: "EUR" }).format(v)
}

function validate(): boolean {
  const e: FieldErrors = {}
  if (!form.customerName.trim()) e.customerName = "Please enter your full name."
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.customerEmail)) e.customerEmail = "Please enter a valid e-mail address."
  if (!form.shippingAddress.trim()) e.shippingAddress = "Please enter your street and number."
  if (!form.shippingCity.trim()) e.shippingCity = "Please enter your city."
  if (!form.shippingPostalCode.trim()) e.shippingPostalCode = "Please enter your postal code."
  if (!form.shippingCountry.trim()) e.shippingCountry = "Please enter your country."
  errors.value = e
  return Object.keys(e).length === 0
}

async function submit() {
  if (cart.isEmpty) return
  if (!validate()) {
    toast.error("Please fix the highlighted fields")
    return
  }
  submitting.value = true
  try {
    const input: OrderInputDto = {
      ...form,
      customerPhone: form.customerPhone || undefined,
      orderLines: cart.lines.map((l) => ({ productId: l.productId, quantity: l.quantity })),
    }
    const order = await createOrder(input)
    cart.clear()
    toast.success("Order placed! Thank you for shopping with us.")
    await router.push({ name: "order-confirmation", params: { id: order.id } })
    return
  } catch (err) {
    if (err instanceof OrderValidationError) {
      errors.value = err.errors
      toast.error(err.message)
    } else {
      toast.error("Something went wrong placing your order. Please try again.")
    }
  } finally {
    submitting.value = false
  }
}

// Guard against landing on /checkout with nothing in the cart (e.g. a stale bookmark/back-nav).
// Deliberately a one-time mount check, not a live watch: a live watch on cart.isEmpty would also
// fire the instant a successful order clears the cart, racing the post-order redirect to the
// confirmation page and sending the shopper back to an empty cart instead.
onMounted(() => {
  if (cart.isEmpty) router.replace({ name: "cart" })
})
</script>

<template>
  <div class="ws-container ws-checkout">
    <h1 class="display-font">Checkout</h1>

    <div class="ws-checkout-layout">
      <form class="ws-checkout-form" @submit.prevent="submit">
        <section class="ws-form-section">
          <h6><i class="bi bi-person"></i> Contact details</h6>
          <div class="ws-field">
            <label for="customerName">Full name</label>
            <input id="customerName" v-model="form.customerName" class="ws-input" :class="{ 'is-invalid': errors.customerName }" placeholder="Jane Doe" />
            <p v-if="errors.customerName" class="ws-error-text">{{ errors.customerName }}</p>
          </div>
          <div class="ws-field-row">
            <div class="ws-field">
              <label for="customerEmail">E-mail address</label>
              <input id="customerEmail" v-model="form.customerEmail" type="email" class="ws-input" :class="{ 'is-invalid': errors.customerEmail }" placeholder="jane@example.com" />
              <p v-if="errors.customerEmail" class="ws-error-text">{{ errors.customerEmail }}</p>
            </div>
            <div class="ws-field">
              <label for="customerPhone">Phone <span class="ws-muted">(optional)</span></label>
              <input id="customerPhone" v-model="form.customerPhone" class="ws-input" placeholder="+32 470 12 34 56" />
            </div>
          </div>
        </section>

        <section class="ws-form-section">
          <h6><i class="bi bi-truck"></i> Shipping address</h6>
          <div class="ws-field">
            <label for="shippingAddress">Street and number</label>
            <input id="shippingAddress" v-model="form.shippingAddress" class="ws-input" :class="{ 'is-invalid': errors.shippingAddress }" placeholder="Main Street 123" />
            <p v-if="errors.shippingAddress" class="ws-error-text">{{ errors.shippingAddress }}</p>
          </div>
          <div class="ws-field-row ws-field-row-3">
            <div class="ws-field">
              <label for="shippingCity">City</label>
              <input id="shippingCity" v-model="form.shippingCity" class="ws-input" :class="{ 'is-invalid': errors.shippingCity }" placeholder="Brussels" />
              <p v-if="errors.shippingCity" class="ws-error-text">{{ errors.shippingCity }}</p>
            </div>
            <div class="ws-field">
              <label for="shippingPostalCode">Postal code</label>
              <input id="shippingPostalCode" v-model="form.shippingPostalCode" class="ws-input" :class="{ 'is-invalid': errors.shippingPostalCode }" placeholder="1000" />
              <p v-if="errors.shippingPostalCode" class="ws-error-text">{{ errors.shippingPostalCode }}</p>
            </div>
            <div class="ws-field">
              <label for="shippingCountry">Country</label>
              <input id="shippingCountry" v-model="form.shippingCountry" class="ws-input" :class="{ 'is-invalid': errors.shippingCountry }" placeholder="Belgium" />
              <p v-if="errors.shippingCountry" class="ws-error-text">{{ errors.shippingCountry }}</p>
            </div>
          </div>
        </section>

        <section class="ws-form-section">
          <h6><i class="bi bi-credit-card"></i> Payment</h6>
          <p class="ws-muted ws-payment-note">
            This is a demo store &mdash; no real payment is collected. Placing the order simulates checkout
            completion.
          </p>
        </section>

        <button type="submit" class="btn-ws btn-ws-primary btn-ws-block" :disabled="submitting || cart.isEmpty">
          <span v-if="submitting"><i class="bi bi-arrow-repeat spin"></i> Placing order...</span>
          <span v-else>Place order &mdash; {{ fmt(total) }}</span>
        </button>
      </form>

      <aside class="ws-checkout-summary">
        <h6>Order summary</h6>
        <ul class="ws-summary-lines">
          <li v-for="line in cart.lines" :key="line.productId">
            <img :src="line.imageUrl ?? ''" :alt="line.title" />
            <div>
              <span class="ws-summary-line-title">{{ line.title }}</span>
              <span class="ws-muted">Qty {{ line.quantity }}</span>
            </div>
            <span class="ws-summary-line-price">{{ fmt(line.price * line.quantity) }}</span>
          </li>
        </ul>
        <div class="ws-summary-row">
          <span>Subtotal</span>
          <span>{{ fmt(cart.subTotal) }}</span>
        </div>
        <div class="ws-summary-row">
          <span>Shipping</span>
          <span>{{ shippingCost === 0 ? "Free" : fmt(shippingCost) }}</span>
        </div>
        <div class="ws-summary-row ws-summary-total">
          <span>Total</span>
          <span>{{ fmt(total) }}</span>
        </div>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.ws-checkout {
  padding: 2.5rem 1.5rem 5rem;
}
.ws-checkout h1 {
  font-size: 1.9rem;
  margin-bottom: 1.6rem;
}
.ws-checkout-layout {
  display: grid;
  grid-template-columns: 1fr 340px;
  gap: 2.5rem;
  align-items: start;
}
@media (max-width: 850px) {
  .ws-checkout-layout {
    grid-template-columns: 1fr;
  }
}
.ws-form-section {
  background: var(--ws-surface);
  border: 1px solid var(--ws-line);
  border-radius: var(--ws-radius);
  padding: 1.4rem 1.5rem;
  margin-bottom: 1.2rem;
}
.ws-form-section h6 {
  font-weight: 800;
  font-size: 0.95rem;
  margin-bottom: 1.1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}
.ws-field {
  margin-bottom: 1rem;
}
.ws-field-row {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
}
.ws-field-row-3 {
  grid-template-columns: 1fr 1fr 1fr;
}
@media (max-width: 560px) {
  .ws-field-row,
  .ws-field-row-3 {
    grid-template-columns: 1fr;
  }
}
.ws-payment-note {
  font-size: 0.85rem;
  margin: 0;
}
.spin {
  display: inline-block;
  animation: ws-spin 0.8s linear infinite;
}
@keyframes ws-spin {
  to {
    transform: rotate(360deg);
  }
}
.ws-checkout-summary {
  background: var(--ws-surface);
  border: 1px solid var(--ws-line);
  border-radius: var(--ws-radius);
  padding: 1.5rem;
  position: sticky;
  top: 6rem;
}
.ws-checkout-summary h6 {
  font-weight: 800;
  margin-bottom: 1rem;
}
.ws-summary-lines {
  list-style: none;
  padding: 0;
  margin: 0 0 1rem;
  display: flex;
  flex-direction: column;
  gap: 0.8rem;
  max-height: 280px;
  overflow-y: auto;
}
.ws-summary-lines li {
  display: flex;
  align-items: center;
  gap: 0.7rem;
}
.ws-summary-lines img {
  width: 42px;
  height: 42px;
  border-radius: var(--ws-radius-sm);
  object-fit: cover;
  background: var(--ws-bg-alt);
}
.ws-summary-lines div {
  flex: 1 1 auto;
  display: flex;
  flex-direction: column;
  font-size: 0.82rem;
  min-width: 0;
}
.ws-summary-line-title {
  font-weight: 600;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}
.ws-summary-line-price {
  font-weight: 700;
  font-size: 0.85rem;
}
.ws-summary-row {
  display: flex;
  justify-content: space-between;
  font-size: 0.9rem;
  padding: 0.35rem 0;
  color: var(--ws-ink-soft);
  border-top: 1px solid var(--ws-line);
}
.ws-summary-row:first-of-type {
  border-top: none;
  padding-top: 0;
}
.ws-summary-total {
  font-weight: 800;
  color: var(--ws-ink);
  font-size: 1.05rem;
}
</style>
