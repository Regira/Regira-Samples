<script setup lang="ts">
import { computed, ref } from "vue"

// Single-series column chart (magnitude over time). One hue, thin columns with rounded data-ends
// anchored to the baseline, recessive gridlines, hover tooltip per column, and a table fallback.
const props = defineProps<{
    points: Array<{ label: string; value: number; note?: string }>
    format: (v: number) => string
    formatAxis?: (v: number) => string
    chartLabel: string
}>()

const hover = ref<number>()
const max = computed(() => {
    const m = Math.max(0, ...props.points.map((p) => p.value))
    if (m <= 0) return 1
    const magnitude = Math.pow(10, Math.floor(Math.log10(m)))
    return Math.ceil(m / magnitude) * magnitude
})
const ticks = computed(() => [0, 0.25, 0.5, 0.75, 1].map((f) => f * max.value))
const showTable = ref(false)
</script>

<template>
    <div class="fleet-colchart">
        <div class="fleet-colchart__plot" role="img" :aria-label="chartLabel">
            <div class="fleet-colchart__grid">
                <div v-for="t in ticks" :key="t" class="fleet-colchart__tick" :style="{ bottom: `${(t / max) * 100}%` }">
                    <span>{{ (formatAxis ?? format)(t) }}</span>
                </div>
            </div>
            <div class="fleet-colchart__cols">
                <div
                    v-for="(p, i) in points"
                    :key="p.label"
                    class="fleet-colchart__slot"
                    @mouseenter="hover = i"
                    @mouseleave="hover = undefined"
                    @focus="hover = i"
                    @blur="hover = undefined"
                    tabindex="0"
                >
                    <div class="fleet-colchart__col" :class="{ 'is-hover': hover === i }" :style="{ height: `${(p.value / max) * 100}%` }"></div>
                    <div v-if="hover === i" class="fleet-tooltip" :class="{ 'fleet-tooltip--left': i > points.length / 2 }">
                        <div class="fleet-tooltip__title">{{ p.label }}</div>
                        <div class="fleet-tooltip__value">{{ format(p.value) }}</div>
                        <div v-if="p.note" class="fleet-tooltip__note">{{ p.note }}</div>
                    </div>
                </div>
            </div>
        </div>
        <div class="fleet-colchart__labels">
            <span v-for="(p, i) in points" :key="p.label" :class="{ 'd-none d-sm-inline': i % 2 === 1 }">{{ p.label }}</span>
        </div>
        <button type="button" class="btn btn-link btn-sm px-0 mt-1" @click="showTable = !showTable">
            <i class="bi bi-table me-1"></i>{{ showTable ? "Hide" : "Show" }} data table
        </button>
        <table v-if="showTable" class="table table-sm small mt-1 mb-0">
            <tbody>
                <tr v-for="p in points" :key="p.label">
                    <td>{{ p.label }}</td>
                    <td class="text-end fleet-num">{{ format(p.value) }}</td>
                    <td class="text-secondary">{{ p.note }}</td>
                </tr>
            </tbody>
        </table>
    </div>
</template>
