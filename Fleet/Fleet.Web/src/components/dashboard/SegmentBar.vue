<script setup lang="ts">
import { computed } from "vue"

// Part-to-whole of a few states: one stacked bar (2px surface gaps between segments) + a legend that
// always carries label + count + share, so identity is never colour-only.
const props = defineProps<{ segments: Array<{ key: string; label: string; value: number; color: string; icon?: string }> }>()
const total = computed(() => props.segments.reduce((s, x) => s + x.value, 0) || 1)
const pct = (v: number) => Math.round((v / total.value) * 100)
</script>

<template>
    <div>
        <div class="fleet-segbar" role="img" :aria-label="segments.map((s) => `${s.label} ${s.value}`).join(', ')">
            <div
                v-for="s in segments.filter((x) => x.value > 0)"
                :key="s.key"
                class="fleet-segbar__seg"
                :style="{ flexGrow: s.value, background: s.color }"
                :title="`${s.label}: ${s.value} (${pct(s.value)}%)`"
            ></div>
        </div>
        <ul class="fleet-legend">
            <li v-for="s in segments" :key="s.key">
                <span class="fleet-legend__swatch" :style="{ background: s.color }"></span>
                <i v-if="s.icon" :class="s.icon" class="text-secondary"></i>
                <span>{{ s.label }}</span>
                <strong class="fleet-num ms-auto">{{ s.value }}</strong>
                <span class="text-secondary fleet-num fleet-legend__pct">{{ pct(s.value) }}%</span>
            </li>
        </ul>
    </div>
</template>
