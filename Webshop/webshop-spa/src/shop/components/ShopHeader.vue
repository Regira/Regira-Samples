<script setup lang="ts">
import { onMounted, ref, watch } from "vue"
import { RouterLink, useRoute, useRouter } from "vue-router"
import { useConfig } from "@/app-config"
import { useCartStore } from "../cart"
import { useShopCatalog } from "../catalog"
import { money } from "../money"

const { title } = useConfig()
const cart = useCartStore()
const { categories, load } = useShopCatalog()
const route = useRoute()
const router = useRouter()
const q = ref("")
const menuOpen = ref(false)

onMounted(load)
watch(
    () => route.query.q,
    (value) => (q.value = typeof value === "string" ? value : ""),
    { immediate: true }
)
watch(() => route.fullPath, () => (menuOpen.value = false))

function search() {
    router.push({ name: "shop", query: q.value.trim() ? { q: q.value.trim() } : {} })
}
const isActiveCategory = (id: number) => route.name === "shop" && String(route.query.categoryId) === String(id)
</script>

<template>
    <div class="ws-header">
        <div class="ws-announce">
            <div class="container-xl d-flex justify-content-center justify-content-md-between gap-4">
                <span><i class="bi bi-truck me-1"></i>Free shipping on orders over {{ money(50) }}</span>
                <span class="d-none d-md-inline"><i class="bi bi-arrow-repeat me-1"></i>30-day free returns</span>
                <span class="d-none d-lg-inline"><i class="bi bi-shield-check me-1"></i>2-year warranty on everything</span>
            </div>
        </div>
        <div class="ws-mainbar">
            <div class="container-xl d-flex align-items-center gap-3">
                <button class="btn btn-link text-body d-lg-none p-1" type="button" aria-label="Menu" @click="menuOpen = !menuOpen">
                    <i class="bi fs-4" :class="menuOpen ? 'bi-x-lg' : 'bi-list'"></i>
                </button>
                <RouterLink :to="{ name: 'home' }" class="ws-logo">
                    <span class="ws-logo__mark"><i class="bi bi-bag-heart-fill"></i></span>
                    <span class="ws-logo__text">{{ $tm(title) }}</span>
                </RouterLink>
                <form class="ws-search flex-grow-1 d-none d-md-flex" role="search" @submit.prevent="search">
                    <i class="bi bi-search"></i>
                    <input v-model="q" type="search" class="form-control" placeholder="Search products, brands, SKUs..." aria-label="Search" />
                    <button class="btn btn-primary" type="submit">Search</button>
                </form>
                <div class="ms-auto d-flex align-items-center gap-1">
                    <RouterLink :to="{ name: 'adminHome' }" class="btn btn-link ws-icon-btn" title="Back office">
                        <i class="bi bi-speedometer2"></i><span class="d-none d-xl-inline ms-1">Back office</span>
                    </RouterLink>
                    <RouterLink :to="{ name: 'cart' }" class="btn ws-cart-btn" title="Shopping cart">
                        <i class="bi bi-bag"></i>
                        <span v-if="cart.count" class="ws-cart-btn__count">{{ cart.count }}</span>
                        <span class="d-none d-sm-inline ms-2">{{ money(cart.subtotal) }}</span>
                    </RouterLink>
                </div>
            </div>
            <form class="container-xl ws-search ws-search--mobile d-md-none mt-2" role="search" @submit.prevent="search">
                <i class="bi bi-search"></i>
                <input v-model="q" type="search" class="form-control" placeholder="Search products..." aria-label="Search" />
            </form>
        </div>
        <nav class="ws-catnav" :class="{ 'is-open': menuOpen }">
            <div class="container-xl">
                <RouterLink :to="{ name: 'shop' }" class="ws-catnav__link" :class="{ active: route.name === 'shop' && !route.query.categoryId && !route.query.onSale }">
                    <i class="bi bi-grid"></i>All products
                </RouterLink>
                <RouterLink
                    v-for="c in categories"
                    :key="c.id"
                    :to="{ name: 'shop', query: { categoryId: c.id } }"
                    class="ws-catnav__link"
                    :class="{ active: isActiveCategory(c.id) }"
                >
                    <i :class="c.$iconClass" :style="{ color: c.color }"></i>{{ c.title }}
                </RouterLink>
                <RouterLink :to="{ name: 'shop', query: { onSale: 'true' } }" class="ws-catnav__link ws-catnav__link--deals" :class="{ active: route.name === 'shop' && route.query.onSale === 'true' }">
                    <i class="bi bi-lightning-charge-fill"></i>Deals
                </RouterLink>
            </div>
        </nav>
    </div>
</template>
