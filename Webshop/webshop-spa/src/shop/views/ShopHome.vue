<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { useAppFeedback } from "@regira/modules/vue/ui"
import { useEntityStore as usePromotionStore, type Entity as Promotion } from "@/entities/promotions"
import { useShopCatalog } from "../catalog"
import PromoBanner from "../components/PromoBanner.vue"
import ProductRail from "../components/ProductRail.vue"

const { service: promotionService, fromPool } = usePromotionStore()
const { categories, load } = useShopCatalog()
const feedback = useAppFeedback()

const promotions = ref<Array<Promotion>>([])
const active = ref(0)
const heroes = computed(() => promotions.value.slice(0, 3))
const strips = computed(() => promotions.value.slice(3, 5))
let timer: number | undefined

onMounted(async () => {
    load()
    try {
        const { items } = await promotionService.search({ live: true, pageSize: 10 })
        promotions.value = fromPool(items)
    } catch (ex) {
        console.error("loading promotions failed", ex)
    }
    timer = window.setInterval(() => (active.value = heroes.value.length ? (active.value + 1) % heroes.value.length : 0), 6500)
})
onBeforeUnmount(() => window.clearInterval(timer))

const email = ref("")
function subscribe() {
    feedback.success(`Thanks! We'll keep ${email.value} posted.`)
    email.value = ""
}
</script>

<template>
    <div class="container-xl">
        <!-- hero carousel (live promotions) -->
        <section class="ws-hero mt-3 mt-md-4">
            <div v-if="heroes.length" class="ws-hero__slides">
                <transition-group name="ws-fade">
                    <PromoBanner v-for="(p, i) in heroes" v-show="i === active" :key="p.id" :promotion="p" />
                </transition-group>
                <div class="ws-hero__dots">
                    <button v-for="(p, i) in heroes" :key="p.id" type="button" :class="{ active: i === active }" :aria-label="p.title" @click="active = i"></button>
                </div>
            </div>
            <div v-else class="ws-skeleton ws-skeleton--hero"></div>
        </section>

        <!-- USPs -->
        <section class="ws-usps">
            <div><i class="bi bi-truck"></i><span><strong>Free shipping</strong> over &euro;50</span></div>
            <div><i class="bi bi-lightning-charge"></i><span><strong>Next-day</strong> express delivery</span></div>
            <div><i class="bi bi-arrow-repeat"></i><span><strong>30 days</strong> to change your mind</span></div>
            <div><i class="bi bi-shield-check"></i><span><strong>2-year</strong> warranty</span></div>
        </section>

        <!-- categories -->
        <section class="mb-5">
            <div class="ws-section-head">
                <h2 class="ws-section-title">Shop by category</h2>
                <RouterLink :to="{ name: 'shop' }" class="ws-link-more">All products <i class="bi bi-chevron-right"></i></RouterLink>
            </div>
            <div class="row row-cols-2 row-cols-sm-3 row-cols-md-5 g-3">
                <div v-for="c in categories" :key="c.id" class="col">
                    <RouterLink :to="{ name: 'shop', query: { categoryId: c.id } }" class="ws-cat-tile" :style="{ '--cat': c.color }">
                        <span class="ws-cat-tile__icon"><i :class="c.$iconClass"></i></span>
                        <span class="ws-cat-tile__title">{{ c.title }}</span>
                        <span class="ws-cat-tile__count">{{ c.productCount }} products</span>
                    </RouterLink>
                </div>
            </div>
        </section>

        <ProductRail title="Staff picks" subtitle="Hand-picked favourites our team can't stop talking about" :query="{ isFeatured: true, inStock: true }" :more-link="{ name: 'shop', query: { isFeatured: 'true' } }" />

        <section v-if="strips.length" class="row g-3 my-4">
            <div v-for="p in strips" :key="p.id" class="col-md-6"><PromoBanner :promotion="p" variant="compact" /></div>
        </section>

        <ProductRail title="Hot deals" subtitle="Limited-time prices on popular products" :query="{ onSale: true, inStock: true, sortBy: ['Popularity'] }" :more-link="{ name: 'shop', query: { onSale: 'true' } }" />
        <ProductRail title="New arrivals" subtitle="Fresh in this month" :query="{ sortBy: ['Newest'] }" :more-link="{ name: 'shop', query: { sortBy: 'Newest' } }" />
        <ProductRail title="Top rated" subtitle="Loved by thousands of reviewers" :query="{ sortBy: ['Rating'], minRating: 4.5 }" :more-link="{ name: 'shop', query: { sortBy: 'Rating', minRating: '4' } }" />

        <!-- newsletter -->
        <section class="ws-newsletter my-5">
            <div>
                <h2>Get 10% off your first order</h2>
                <p class="mb-0">Subscribe for early access to deals, new arrivals and the occasional good read.</p>
            </div>
            <form class="d-flex gap-2" @submit.prevent="subscribe">
                <input v-model="email" type="email" class="form-control" placeholder="you@example.com" required aria-label="Email address" />
                <button class="btn btn-light fw-semibold text-nowrap" type="submit">Subscribe</button>
            </form>
        </section>
    </div>
</template>
