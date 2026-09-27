<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue"
import { RouterLink, useRouter } from "vue-router"
import { Feedback, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useEntityStore as useOrderStore, type Entity as Order } from "@/entities/orders"
import OrderEntity from "@/entities/orders/data/Entity"
import OrderLine from "@/entities/orders/order-lines/Entity"
import { useEntityStore as useProductStore } from "@/entities/products"
import { useCartStore } from "../cart"
import { money, shippingCost, shippingOptions, paymentOptions, countries, type ShippingKey } from "../money"
import ProductVisual from "../components/ProductVisual.vue"

const DETAILS_KEY = "nordlys.checkout.v1"
const cart = useCartStore()
const { service: orderService } = useOrderStore()
const { service: productService } = useProductStore()
const router = useRouter()
const feedback = useFeedback()

type Details = Pick<Order, "customerName" | "email" | "phone" | "street" | "postalCode" | "city" | "country" | "notes"> & {
    shippingMethod: ShippingKey
    paymentMethod: Order["paymentMethod"]
}
function loadDetails(): Partial<Details> {
    try {
        return JSON.parse(localStorage.getItem(DETAILS_KEY) || "{}")
    } catch {
        return {}
    }
}
const form = reactive<Details>({
    customerName: "",
    email: "",
    phone: "",
    street: "",
    postalCode: "",
    city: "",
    country: "Belgium",
    notes: "",
    shippingMethod: "Standard",
    paymentMethod: "Card",
    ...loadDetails(),
})
const remember = ref(true)
const accepted = ref(false)

onMounted(async () => {
    if (!cart.lines.length) return
    try {
        const { items } = await productService.search({ ids: cart.lines.map((l) => l.productId), pageSize: 0 })
        cart.refresh(items)
    } catch (ex) {
        console.error("refreshing the cart failed", ex)
    }
})

const shipping = computed(() => shippingCost(form.shippingMethod, cart.subtotal, cart.count))
const total = computed(() => cart.subtotal + shipping.value)
const canSubmit = computed(() => cart.lines.length > 0 && !cart.unavailableLines.length && accepted.value && !feedback.isPending)

async function placeOrder() {
    if (!canSubmit.value) return
    feedback.pending("Placing your order...")
    const order = Object.assign(new OrderEntity(), {
        status: "Pending",
        customerName: form.customerName.trim(),
        email: form.email.trim(),
        phone: form.phone?.trim() || undefined,
        street: form.street.trim(),
        postalCode: form.postalCode.trim(),
        city: form.city.trim(),
        country: form.country,
        notes: form.notes?.trim() || undefined,
        shippingMethod: form.shippingMethod,
        paymentMethod: form.paymentMethod,
        // only product + quantity travel: the API prices every line itself
        orderLines: cart.lines.map((l) => OrderLine.create({ productId: l.productId, quantity: l.quantity })),
    }) as Order
    try {
        const result = await orderService.save(order)
        const saved = result?.saved
        if (!saved?.id) throw new Error("The order was not saved")
        try {
            if (remember.value) {
                const { notes: _notes, ...keep } = form
                localStorage.setItem(DETAILS_KEY, JSON.stringify(keep))
            } else localStorage.removeItem(DETAILS_KEY)
        } catch {
            /* storage unavailable */
        }
        cart.clear()
        feedback.reset()
        router.push({ name: "orderConfirmation", params: { id: saved.id } })
    } catch (ex) {
        console.error("placing the order failed", ex)
        feedback.fail("We couldn't place your order", toFeedbackError(ex))
    }
}
</script>

<template>
    <div class="container-xl py-3 py-md-4">
        <ol class="ws-steps mb-4">
            <li class="done"><RouterLink :to="{ name: 'cart' }"><i class="bi bi-bag-check"></i>Cart</RouterLink></li>
            <li class="current"><span><i class="bi bi-person-lines-fill"></i>Details &amp; payment</span></li>
            <li><span><i class="bi bi-check2-circle"></i>Confirmation</span></li>
        </ol>

        <div v-if="!cart.lines.length" class="ws-empty">
            <i class="bi bi-bag"></i>
            <h5>Your cart is empty</h5>
            <RouterLink :to="{ name: 'shop' }" class="btn btn-primary">Start shopping</RouterLink>
        </div>

        <form v-else class="row g-4" @submit.prevent="placeOrder">
            <div class="col-lg-7">
                <section class="ws-panel mb-3">
                    <h5 class="ws-panel__title"><span class="ws-step-no">1</span>Contact</h5>
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label" for="c-name">Full name</label>
                            <input id="c-name" v-model="form.customerName" class="form-control" required maxlength="128" autocomplete="name" />
                        </div>
                        <div class="col-md-7">
                            <label class="form-label" for="c-email">Email</label>
                            <input id="c-email" v-model="form.email" type="email" class="form-control" required maxlength="256" autocomplete="email" />
                        </div>
                        <div class="col-md-5">
                            <label class="form-label" for="c-phone">Phone <small class="text-body-secondary">(optional)</small></label>
                            <input id="c-phone" v-model="form.phone" type="tel" class="form-control" maxlength="32" autocomplete="tel" />
                        </div>
                    </div>
                </section>

                <section class="ws-panel mb-3">
                    <h5 class="ws-panel__title"><span class="ws-step-no">2</span>Shipping address</h5>
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label" for="a-street">Street and number</label>
                            <input id="a-street" v-model="form.street" class="form-control" required maxlength="256" autocomplete="street-address" />
                        </div>
                        <div class="col-sm-4">
                            <label class="form-label" for="a-zip">Postal code</label>
                            <input id="a-zip" v-model="form.postalCode" class="form-control" required maxlength="16" autocomplete="postal-code" />
                        </div>
                        <div class="col-sm-8">
                            <label class="form-label" for="a-city">City</label>
                            <input id="a-city" v-model="form.city" class="form-control" required maxlength="128" autocomplete="address-level2" />
                        </div>
                        <div class="col-12">
                            <label class="form-label" for="a-country">Country</label>
                            <select id="a-country" v-model="form.country" class="form-select" required>
                                <option v-for="c in countries" :key="c" :value="c">{{ c }}</option>
                            </select>
                        </div>
                    </div>
                </section>

                <section class="ws-panel mb-3">
                    <h5 class="ws-panel__title"><span class="ws-step-no">3</span>Delivery</h5>
                    <div class="ws-options">
                        <label v-for="o in shippingOptions" :key="o.key" class="ws-option" :class="{ active: form.shippingMethod === o.key }">
                            <input v-model="form.shippingMethod" type="radio" name="shipping" :value="o.key" class="form-check-input" />
                            <i :class="o.icon" class="ws-option__icon"></i>
                            <span class="ws-option__text"><strong>{{ o.title }}</strong><small>{{ o.eta }}</small></span>
                            <span class="ws-option__price">{{ shippingCost(o.key, cart.subtotal, cart.count) === 0 ? "Free" : money(shippingCost(o.key, cart.subtotal, cart.count)) }}</span>
                        </label>
                    </div>
                </section>

                <section class="ws-panel mb-3">
                    <h5 class="ws-panel__title"><span class="ws-step-no">4</span>Payment</h5>
                    <div class="ws-options ws-options--grid">
                        <label v-for="o in paymentOptions" :key="o.key" class="ws-option" :class="{ active: form.paymentMethod === o.key }">
                            <input v-model="form.paymentMethod" type="radio" name="payment" :value="o.key" class="form-check-input" />
                            <i :class="o.icon" class="ws-option__icon"></i>
                            <span class="ws-option__text"><strong>{{ o.title }}</strong></span>
                        </label>
                    </div>
                    <p class="small text-body-secondary mt-3 mb-0">
                        <i class="bi bi-info-circle me-1"></i>This is a demo store: no payment details are requested and nothing is charged.
                    </p>
                </section>

                <section class="ws-panel">
                    <label class="form-label" for="o-notes">Delivery notes <small class="text-body-secondary">(optional)</small></label>
                    <textarea id="o-notes" v-model="form.notes" class="form-control" rows="2" maxlength="1024" placeholder="e.g. leave the parcel with the neighbours"></textarea>
                </section>
            </div>

            <div class="col-lg-5">
                <div class="ws-panel ws-summary ws-sticky">
                    <h5 class="mb-3">Your order</h5>
                    <div v-for="line in cart.lines" :key="line.productId" class="ws-mini-line">
                        <div class="ws-mini-line__media">
                            <ProductVisual :seed="line.productId" :icon="line.product.categoryIcon" :color="line.product.categoryColor" :image-url="line.product.imageUrl" size="sm" />
                            <span class="ws-mini-line__qty">{{ line.quantity }}</span>
                        </div>
                        <div class="ws-mini-line__title">
                            {{ line.product.title }}
                            <div v-if="!line.product.isActive || line.product.stock < line.quantity" class="small text-danger">Unavailable in this quantity</div>
                        </div>
                        <div class="ws-mini-line__price">{{ money(line.product.price * line.quantity) }}</div>
                    </div>
                    <hr />
                    <div class="ws-summary__row"><span>Subtotal</span><span>{{ money(cart.subtotal) }}</span></div>
                    <div v-if="cart.savings > 0" class="ws-summary__row text-success"><span>Savings included</span><span>{{ money(cart.savings) }}</span></div>
                    <div class="ws-summary__row"><span>Shipping</span><span>{{ shipping === 0 ? "Free" : money(shipping) }}</span></div>
                    <div class="ws-summary__row ws-summary__total"><span>Total <small class="fw-normal text-body-secondary">incl. VAT</small></span><span>{{ money(total) }}</span></div>

                    <div class="form-check mt-3">
                        <input id="remember" v-model="remember" class="form-check-input" type="checkbox" />
                        <label class="form-check-label small" for="remember">Remember my details on this device</label>
                    </div>
                    <div class="form-check mt-1">
                        <input id="accept" v-model="accepted" class="form-check-input" type="checkbox" required />
                        <label class="form-check-label small" for="accept">I agree to the terms of sale and the 30-day return policy</label>
                    </div>

                    <Feedback :feedback="feedback" class="mt-3" />

                    <button type="submit" class="btn btn-primary btn-lg w-100 mt-3" :disabled="!canSubmit">
                        <span v-if="feedback.isPending" class="spinner-border spinner-border-sm me-2"></span>
                        <i v-else class="bi bi-lock-fill me-2"></i>Place order - {{ money(total) }}
                    </button>
                    <RouterLink :to="{ name: 'cart' }" class="btn btn-link w-100 mt-1"><i class="bi bi-arrow-left me-1"></i>Back to cart</RouterLink>
                </div>
            </div>
        </form>
    </div>
</template>
