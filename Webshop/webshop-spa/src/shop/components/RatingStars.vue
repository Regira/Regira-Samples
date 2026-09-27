<script setup lang="ts">
import { computed } from "vue"
const props = withDefaults(defineProps<{ rating?: number; count?: number; small?: boolean }>(), { rating: 0 })
const stars = computed(() =>
    [1, 2, 3, 4, 5].map((i) => (props.rating >= i - 0.25 ? "bi-star-fill" : props.rating >= i - 0.75 ? "bi-star-half" : "bi-star"))
)
</script>

<template>
    <span class="ws-rating" :class="{ 'small': small }" :title="count ? `${rating.toFixed(1)} out of 5 (${count} reviews)` : 'No reviews yet'">
        <template v-if="count">
            <i v-for="(s, i) in stars" :key="i" class="bi" :class="s"></i>
            <span class="ws-rating__value ms-1">{{ rating.toFixed(1) }}</span>
            <span class="text-body-secondary ms-1">({{ count.toLocaleString() }})</span>
        </template>
        <span v-else class="text-body-secondary">No reviews yet</span>
    </span>
</template>
