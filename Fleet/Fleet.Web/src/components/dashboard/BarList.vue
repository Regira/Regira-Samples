<script setup lang="ts">
import { computed } from "vue"
import type { RouteLocationRaw } from "vue-router"

// Horizontal bars for ranking/magnitude by category: one hue, label + value as text (text never wears the
// series colour), bar length relative to the largest row. Native title gives the hover detail.
const props = defineProps<{
    rows: Array<{ key: string | number; label: string; value: number; note?: string; icon?: string; to?: RouteLocationRaw }>
    format: (v: number) => string
}>()
const max = computed(() => Math.max(1, ...props.rows.map((r) => r.value)))
</script>

<template>
    <ul class="fleet-barlist">
        <li v-for="r in rows" :key="r.key" :title="`${r.label}: ${format(r.value)}${r.note ? ' - ' + r.note : ''}`">
            <div class="fleet-barlist__text">
                <span class="text-truncate">
                    <i v-if="r.icon" :class="r.icon" class="me-1 text-secondary"></i>
                    <router-link v-if="r.to" :to="r.to">{{ r.label }}</router-link>
                    <template v-else>{{ r.label }}</template>
                    <small v-if="r.note" class="text-secondary ms-1">{{ r.note }}</small>
                </span>
                <span class="fleet-num fw-semibold">{{ format(r.value) }}</span>
            </div>
            <div class="fleet-barlist__track"><div class="fleet-barlist__bar" :style="{ width: `${(r.value / max) * 100}%` }"></div></div>
        </li>
    </ul>
</template>
