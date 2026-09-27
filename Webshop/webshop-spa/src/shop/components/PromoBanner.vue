<!-- A promotional banner (hero or compact) rendered from a Promotion -->
<script setup lang="ts">
import { computed } from "vue"
import { RouterLink } from "vue-router"
import type { Entity as Promotion } from "@/entities/promotions"

const props = withDefaults(defineProps<{ promotion: Promotion; variant?: "hero" | "compact" }>(), { variant: "hero" })
const link = computed(() => props.promotion.ctaLink || "/shop")
const isInternal = computed(() => link.value.startsWith("/"))
</script>

<template>
    <div class="ws-promo" :class="[`ws-theme-${promotion.theme || 'midnight'}`, `ws-promo--${variant}`]">
        <div class="ws-promo__content">
            <span v-if="promotion.badge" class="ws-promo__badge">{{ promotion.badge }}</span>
            <h2 class="ws-promo__title">{{ promotion.title }}</h2>
            <p v-if="promotion.subtitle" class="ws-promo__subtitle">{{ promotion.subtitle }}</p>
            <template v-if="promotion.ctaLabel">
                <RouterLink v-if="isInternal" :to="link" class="btn ws-promo__cta">
                    {{ promotion.ctaLabel }} <i class="bi bi-arrow-right ms-1"></i>
                </RouterLink>
                <a v-else :href="link" class="btn ws-promo__cta">{{ promotion.ctaLabel }} <i class="bi bi-arrow-right ms-1"></i></a>
            </template>
        </div>
        <i class="ws-promo__art bi" :class="`bi-${promotion.icon || 'stars'}`" aria-hidden="true"></i>
    </div>
</template>
