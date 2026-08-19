<template>
    <span class="badge rounded-pill" :class="badgeClass">{{ label }}</span>
</template>

<script setup lang="ts">
import { computed } from "vue"

const props = defineProps<{
    status?: string
    // maps a status value -> a Bootstrap contextual color (success/warning/danger/secondary/info/primary)
    variants: Record<string, string>
    label?: string
}>()

const badgeClass = computed(() => `text-bg-${props.status ? (props.variants[props.status] ?? "secondary") : "secondary"}`)
const label = computed(() => props.label ?? props.status ?? "-")
</script>
