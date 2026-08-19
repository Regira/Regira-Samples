<template>
    <div class="balance-bar">
        <div class="progress" :style="{ height: compact ? '10px' : '18px' }">
            <div
                class="progress-bar"
                :class="usedBarClass"
                role="progressbar"
                :style="{ width: usedPct + '%' }"
                :aria-valuenow="usedPct"
                aria-valuemin="0"
                aria-valuemax="100"
            ></div>
        </div>
        <div v-if="!compact" class="d-flex justify-content-between small text-muted mt-1">
            <span>{{ balance.approvedCredits }} used</span>
            <span v-if="balance.pendingCredits > 0">{{ balance.pendingCredits }} pending</span>
            <span>{{ available }} available</span>
        </div>
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import type { EmployeeBalance } from "./types"

const props = defineProps<{ balance: EmployeeBalance; compact?: boolean }>()

const available = computed(() => props.balance.freelyAvailableCredits + props.balance.carriedOverCredits)
const usedPct = computed(() => {
    const total = Math.max(available.value, 1)
    return Math.min(100, Math.max(0, Math.round((props.balance.approvedCredits / total) * 100)))
})
const usedBarClass = computed(() => {
    if (props.balance.remainingCredits < 0) return "bg-danger"
    if (usedPct.value >= 90) return "bg-warning"
    return "bg-success"
})
</script>
