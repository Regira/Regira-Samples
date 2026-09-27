<script setup lang="ts">
import { computed, onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer } from "@regira/modules/vue/ui"
import { formatDateTime } from "@regira/modules/vue/formatters"
import { useEntityStore as useOrderStore, type Entity as Order } from "@/entities/orders"
import { OrderStatuses } from "@/entities/orders/data/Entity"
import { money, shippingOptions, paymentOptions } from "../money"
import ProductVisual from "../components/ProductVisual.vue"

const props = defineProps<{ id: string }>()
const { service } = useOrderStore()
const order = ref<Order>()
const isLoading = ref(true)

onMounted(async () => {
    try {
        order.value = await service.details(props.id)
    } catch (ex) {
        console.error("loading the order failed", ex)
    } finally {
        isLoading.value = false
    }
})
const timeline = OrderStatuses.filter((s) => s !== "Cancelled")
const reached = computed(() => (order.value ? timeline.indexOf(order.value.status as (typeof timeline)[number]) : -1))
const shippingTitle = computed(() => shippingOptions.find((o) => o.key === order.value?.shippingMethod)?.title)
const paymentTitle = computed(() => paymentOptions.find((o) => o.key === order.value?.paymentMethod)?.title)
</script>

<template>
    <div class="container-xl py-3 py-md-4">
        <ol class="ws-steps mb-4">
            <li class="done"><span><i class="bi bi-bag-check"></i>Cart</span></li>
            <li class="done"><span><i class="bi bi-person-lines-fill"></i>Details &amp; payment</span></li>
            <li class="current"><span><i class="bi bi-check2-circle"></i>Confirmation</span></li>
        </ol>
        <LoadingContainer :is-loading="isLoading">
            <div v-if="!order" class="ws-empty">
                <i class="bi bi-question-circle"></i>
                <h5>We couldn't find this order</h5>
                <RouterLink :to="{ name: 'shop' }" class="btn btn-primary">Back to the shop</RouterLink>
            </div>
            <template v-else>
                <div class="ws-confirm">
                    <div class="ws-confirm__icon"><i class="bi bi-check-lg"></i></div>
                    <h1 class="ws-page-title">Thank you, {{ order.customerName.split(" ")[0] }}!</h1>
                    <p class="lead mb-1">Your order <strong class="ws-code">{{ order.code }}</strong> has been placed.</p>
                    <p class="text-body-secondary">A confirmation will be sent to {{ order.email }}. Placed on {{ formatDateTime(order.created!, "dd/MM/yyyy HH:mm") }}.</p>
                </div>

                <div v-if="order.status !== 'Cancelled'" class="ws-timeline my-4">
                    <div v-for="(s, i) in timeline" :key="s" class="ws-timeline__step" :class="{ done: i <= reached }">
                        <span class="ws-timeline__dot"><i class="bi" :class="i <= reached ? 'bi-check' : 'bi-circle'"></i></span>
                        <span>{{ s }}</span>
                    </div>
                </div>
                <div v-else class="alert alert-danger">This order was cancelled.</div>

                <div class="row g-4">
                    <div class="col-lg-7">
                        <div class="ws-panel">
                            <h5 class="mb-3">Items</h5>
                            <div v-for="line in order.orderLines" :key="line.id" class="ws-mini-line">
                                <div class="ws-mini-line__media">
                                    <ProductVisual :seed="line.productId" :icon="line.product?.category?.icon" :color="line.product?.category?.color" :image-url="line.product?.imageUrl" size="sm" />
                                    <span class="ws-mini-line__qty">{{ line.quantity }}</span>
                                </div>
                                <div class="ws-mini-line__title">
                                    <RouterLink :to="{ name: 'product', params: { id: line.productId } }">{{ line.productTitle }}</RouterLink>
                                    <div class="small text-body-secondary">{{ line.quantity }} x {{ money(line.unitPrice) }}</div>
                                </div>
                                <div class="ws-mini-line__price">{{ money(line.lineTotal) }}</div>
                            </div>
                            <hr />
                            <div class="ws-summary__row"><span>Subtotal</span><span>{{ money(order.subtotal) }}</span></div>
                            <div class="ws-summary__row"><span>Shipping</span><span>{{ order.shippingCost ? money(order.shippingCost) : "Free" }}</span></div>
                            <div class="ws-summary__row ws-summary__total"><span>Total</span><span>{{ money(order.total) }}</span></div>
                        </div>
                    </div>
                    <div class="col-lg-5">
                        <div class="ws-panel mb-3">
                            <h6 class="text-uppercase small text-body-secondary">Delivery</h6>
                            <p class="mb-1 fw-semibold">{{ order.customerName }}</p>
                            <p class="mb-2">{{ order.street }}<br />{{ order.postalCode }} {{ order.city }}<br />{{ order.country }}</p>
                            <p class="mb-0"><i class="bi bi-truck me-1"></i>{{ shippingTitle }}</p>
                        </div>
                        <div class="ws-panel mb-3">
                            <h6 class="text-uppercase small text-body-secondary">Payment</h6>
                            <p class="mb-0">{{ paymentTitle }}</p>
                        </div>
                        <RouterLink :to="{ name: 'shop' }" class="btn btn-primary w-100">Continue shopping</RouterLink>
                    </div>
                </div>
            </template>
        </LoadingContainer>
    </div>
</template>
