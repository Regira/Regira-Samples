<!-- Progress indicator for a credit balance: used (approved) + pending against the free budget,
     with the overdraft zone (down to the minimum balance) shown beyond it. -->
<script setup lang="ts">
import { computed } from "vue"
import { formatCredits } from "@/domain/enums"

const props = withDefaults(
    defineProps<{ free: number; used: number; pending?: number; minBalance?: number; compact?: boolean; showLegend?: boolean }>(),
    { pending: 0, minBalance: -10, compact: false, showLegend: false }
)

// the bar spans the free budget plus the allowed overdraft
const span = computed(() => Math.max(props.free - props.minBalance, props.used + props.pending, 1))
const pct = (v: number) => `${Math.max(0, Math.min(100, (v / span.value) * 100))}%`
const usedWithin = computed(() => Math.min(props.used, Math.max(props.free, 0)))
const overdraft = computed(() => Math.max(0, props.used - Math.max(props.free, 0)))
const pendingPart = computed(() => props.pending)
const remaining = computed(() => props.free - props.used)
const usedPct = computed(() => (props.free > 0 ? Math.round((props.used / props.free) * 100) : 0))
const tone = computed(() => (remaining.value < 0 ? "danger" : remaining.value < 3 ? "warning" : "success"))
</script>

<template>
    <div class="qc-credit-bar">
        <div class="progress-stacked" :class="compact ? 'qc-bar-sm' : 'qc-bar'" :title="`${formatCredits(used)} used, ${formatCredits(pending)} pending of ${formatCredits(free)} free credits`">
            <div class="progress" role="progressbar" :style="{ width: pct(usedWithin) }" :aria-valuenow="usedWithin">
                <div class="progress-bar bg-primary"></div>
            </div>
            <div class="progress" role="progressbar" :style="{ width: pct(overdraft) }" :aria-valuenow="overdraft">
                <div class="progress-bar bg-danger"></div>
            </div>
            <div class="progress" role="progressbar" :style="{ width: pct(pendingPart) }" :aria-valuenow="pendingPart">
                <div class="progress-bar bg-warning progress-bar-striped"></div>
            </div>
        </div>
        <div v-if="!compact" class="d-flex justify-content-between small mt-1">
            <span class="text-muted">{{ usedPct }}% used</span>
            <span :class="`text-${tone} fw-semibold`">{{ formatCredits(remaining) }} left</span>
        </div>
        <div v-if="showLegend" class="d-flex flex-wrap gap-3 small text-muted mt-1">
            <span><i class="qc-dot bg-primary"></i> Used {{ formatCredits(used) }}</span>
            <span><i class="qc-dot bg-warning"></i> Pending {{ formatCredits(pending) }}</span>
            <span v-if="overdraft > 0"><i class="qc-dot bg-danger"></i> Overdraft {{ formatCredits(overdraft) }}</span>
            <span><i class="qc-dot bg-body-secondary border"></i> Free budget {{ formatCredits(free) }}</span>
        </div>
    </div>
</template>

<style scoped>
.qc-bar {
    height: 0.9rem;
}
.qc-bar-sm {
    height: 0.5rem;
}
.qc-dot {
    display: inline-block;
    width: 0.65rem;
    height: 0.65rem;
    border-radius: 50%;
    margin-right: 0.2rem;
    vertical-align: -0.05rem;
}
</style>
