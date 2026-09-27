<script setup lang="ts">
import { computed } from "vue"
import type { BadgeInfo } from "@/infrastructure/domain"

// A status is never colour-only: icon + label + tint.
const props = defineProps<{ value?: string; map: Record<string, BadgeInfo>; compact?: boolean }>()
const info = computed(() => (props.value ? props.map[props.value] : undefined))
</script>

<template>
    <span v-if="info" class="fleet-badge" :class="info.css" :title="info.label">
        <i :class="info.icon" aria-hidden="true"></i><span :class="{ 'd-none d-xl-inline': compact }">{{ info.label }}</span>
    </span>
</template>
