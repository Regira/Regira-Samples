<script setup lang="ts">
import type { RouteLocationRaw } from "vue-router"

// Stat tile: label, headline value, optional context line; a status cue always pairs icon + text.
defineProps<{
    label: string
    value: string
    icon: string
    sub?: string
    status?: "good" | "warning" | "critical"
    statusText?: string
    to?: RouteLocationRaw
}>()
</script>

<template>
    <component :is="to ? 'router-link' : 'div'" :to="to" class="fleet-kpi" :class="{ 'fleet-kpi--link': !!to }">
        <div class="fleet-kpi__top">
            <span class="fleet-kpi__label">{{ label }}</span>
            <span class="fleet-kpi__icon"><i :class="icon"></i></span>
        </div>
        <div class="fleet-kpi__value">{{ value }}</div>
        <div class="fleet-kpi__sub">
            <span v-if="status && statusText" class="fleet-badge" :class="`fleet-badge--${status}`">
                <i :class="status === 'good' ? 'bi bi-check-circle' : status === 'warning' ? 'bi bi-exclamation-circle' : 'bi bi-exclamation-triangle'"></i>
                <span>{{ statusText }}</span>
            </span>
            <span v-if="sub" class="text-secondary">{{ sub }}</span>
        </div>
    </component>
</template>
