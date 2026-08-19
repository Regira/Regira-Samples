<template>
    <div class="card kpi-card h-100 border-0 shadow-sm">
        <div class="card-body d-flex align-items-center gap-3">
            <div class="kpi-icon rounded-circle d-flex align-items-center justify-content-center" :class="`bg-${variant}-subtle text-${variant}`">
                <Icon :name="icon" size="lg" />
            </div>
            <div class="flex-grow-1 text-truncate">
                <div class="kpi-value fw-bold">{{ value }}</div>
                <div class="kpi-label text-muted small text-truncate">{{ label }}</div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
import { Icon } from "@regira/modules/vue/ui"

defineProps<{
    label: string
    value: string | number
    icon: string
    variant?: string
}>()
</script>

<style scoped>
.kpi-icon {
    width: 3rem;
    height: 3rem;
    flex-shrink: 0;
}
.kpi-value {
    font-size: 1.5rem;
    line-height: 1.2;
}
</style>
