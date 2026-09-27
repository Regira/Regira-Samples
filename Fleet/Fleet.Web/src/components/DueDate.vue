<script setup lang="ts">
import { computed } from "vue"
import { daysFromToday, fmtDay } from "@/infrastructure/domain"

// A due date with its state: overdue (critical), due soon (warning) or plain. Icon + text, never colour alone.
const props = withDefaults(defineProps<{ value?: string | null; soonDays?: number; inactive?: boolean }>(), { soonDays: 30 })
const days = computed(() => daysFromToday(props.value))
const state = computed(() => {
    if (props.inactive || days.value == null) return "none"
    if (days.value < 0) return "overdue"
    if (days.value <= props.soonDays) return "soon"
    return "none"
})
const hint = computed(() => {
    if (days.value == null) return ""
    if (days.value < 0) return `${-days.value}d overdue`
    if (days.value === 0) return "today"
    return `in ${days.value}d`
})
</script>

<template>
    <span v-if="value" class="fleet-due" :class="`fleet-due--${state}`" :title="hint">
        <i v-if="state === 'overdue'" class="bi bi-exclamation-circle-fill" aria-hidden="true"></i>
        <i v-else-if="state === 'soon'" class="bi bi-clock-history" aria-hidden="true"></i>
        {{ fmtDay(value) }}
        <small v-if="state !== 'none'" class="fleet-due__hint">{{ hint }}</small>
    </span>
    <span v-else class="text-muted">-</span>
</template>
