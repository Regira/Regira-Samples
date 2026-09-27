<script setup lang="ts">
import { computed } from "vue"
import { useLang } from "@regira/modules/vue/lang"

// Due-date state of a ticket: overdue (red), due within 4h (amber), else the remaining time.
const props = defineProps<{ dueDate?: Date | string; closedAt?: Date | string; isClosed?: boolean; compact?: boolean }>()
const { translate } = useLang()

const due = computed(() => (props.dueDate ? new Date(props.dueDate) : undefined))
const state = computed(() => {
    if (!due.value) return undefined
    if (props.isClosed) {
        const closed = props.closedAt ? new Date(props.closedAt) : undefined
        return closed && closed > due.value ? "breached" : "met"
    }
    const hours = (due.value.getTime() - Date.now()) / 36e5
    return hours < 0 ? "overdue" : hours < 4 ? "soon" : "ok"
})
function span(ms: number) {
    const h = Math.abs(ms) / 36e5
    return h >= 48 ? `${Math.round(h / 24)}d` : h >= 1 ? `${Math.round(h)}h` : `${Math.max(1, Math.round(h * 60))}m`
}
const label = computed(() => {
    if (!due.value || !state.value) return ""
    const diff = due.value.getTime() - Date.now()
    switch (state.value) {
        case "overdue":
            return translate("overdueBy", { time: span(diff) })
        case "soon":
        case "ok":
            return translate("dueIn", { time: span(diff) })
        case "breached":
            return translate("slaBreached")
        default:
            return translate("slaMet")
    }
})
const cls = computed(() =>
    state.value === "overdue" || state.value === "breached" ? "hd-sla-overdue" : state.value === "soon" ? "hd-sla-soon" : "text-muted"
)
const icon = computed(() => (state.value === "met" ? "bi bi-check2-circle" : state.value === "overdue" || state.value === "breached" ? "bi bi-alarm" : "bi bi-clock"))
</script>
<template>
    <span v-if="state" :class="cls" class="small text-nowrap" :title="due?.toLocaleString()">
        <i :class="icon" class="me-1"></i><span v-if="!compact || state !== 'met'">{{ label }}</span>
    </span>
</template>
