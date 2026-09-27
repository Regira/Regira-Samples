<script setup lang="ts">
defineProps<{ value?: number | null }>()
</script>

<template>
    <span v-if="value" class="fleet-rating" :title="`${value} / 5`" :aria-label="`${value} out of 5`">
        <i v-for="n in 5" :key="n" :class="n <= value ? 'bi bi-star-fill' : 'bi bi-star'"></i>
    </span>
    <span v-else class="text-muted small">not rated</span>
</template>
