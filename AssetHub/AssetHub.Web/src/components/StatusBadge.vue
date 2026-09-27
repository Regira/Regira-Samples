<!-- Color-coded status indicator: a tinted pill with a solid dot, colored by the admin-managed AssetStatus.color -->
<script setup lang="ts">
import { computed } from "vue"

const props = defineProps<{ status?: { title?: string; color?: string } | null; small?: boolean }>()
const color = computed(() => props.status?.color || "#6c757d")
</script>

<template>
    <span v-if="status" class="ah-status" :class="{ 'ah-status--sm': small }" :style="{ '--ah-status-color': color }" :title="status.title">
        <span class="ah-status__dot"></span>
        <span class="ah-status__label">{{ status.title }}</span>
    </span>
</template>
