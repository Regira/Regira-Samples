<!-- A titled, horizontally scrolling row of product cards fed by a product search -->
<script setup lang="ts">
import { onMounted, ref, watch } from "vue"
import { RouterLink, type RouteLocationRaw } from "vue-router"
import { useEntityStore as useProductStore, type Entity as Product } from "@/entities/products"
import ProductCard from "./ProductCard.vue"

const props = withDefaults(defineProps<{ title: string; subtitle?: string; query?: Record<string, any>; moreLink?: RouteLocationRaw; size?: number }>(), {
    size: 10,
})
const { service, fromPool } = useProductStore()
const items = ref<Array<Product>>()
const failed = ref(false)

async function load() {
    try {
        const { items: result } = await service.search({ isActive: true, ...(props.query ?? {}), pageSize: props.size, page: 1 })
        items.value = fromPool(result)
    } catch (ex) {
        console.error("loading products failed", ex)
        failed.value = true
    }
}
onMounted(load)
watch(() => JSON.stringify(props.query), load)
</script>

<template>
    <section class="ws-rail">
        <div class="ws-section-head">
            <div>
                <h2 class="ws-section-title">{{ title }}</h2>
                <p v-if="subtitle" class="ws-section-subtitle">{{ subtitle }}</p>
            </div>
            <RouterLink v-if="moreLink" :to="moreLink" class="ws-link-more">View all <i class="bi bi-chevron-right"></i></RouterLink>
        </div>
        <div v-if="items == null && !failed" class="ws-rail__track">
            <div v-for="i in 5" :key="i" class="ws-rail__item"><div class="ws-skeleton"></div></div>
        </div>
        <div v-else-if="items?.length" class="ws-rail__track">
            <div v-for="p in items" :key="p.id" class="ws-rail__item"><ProductCard :product="p" /></div>
        </div>
        <p v-else class="text-body-secondary">Nothing to show right now.</p>
    </section>
</template>
