<script setup lang="ts">
import { RouterLink } from "vue-router"

defineProps<{ category: { title?: string; slug?: string; color?: string } }>()
</script>

<template>
    <RouterLink :to="{ name: 'blog', query: { category: category.slug } }" class="category-badge" :style="{ '--cat-color': category.color || '#6b7280' }">
        {{ category.title }}
    </RouterLink>
</template>
