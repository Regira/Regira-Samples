<script setup lang="ts">
import { computed } from "vue"
import { useScreenTexts } from "../texts"

// A gilded gauge for brains. The needle passes the top of the scale around question 7 and keeps going.
const props = defineProps<{ level: number; max?: number }>()
const angle = computed(() => {
    const max = props.max ?? 10
    // -90deg (left) ... +90deg (right) is the scale; the needle is allowed to overshoot to +150deg
    return Math.min(-90 + (props.level / (max * 0.7)) * 180, 150)
})
const overflow = computed(() => angle.value > 90)
const { t } = useScreenTexts()
</script>

<template>
    <div class="vsg-meter" :class="{ 'is-overflow': overflow }" role="img" :aria-label="t('meterAria', { level })">
        <svg viewBox="0 0 200 128" width="100%" height="100%">
            <defs>
                <linearGradient id="vsgMeterScale" x1="0" x2="1">
                    <stop offset="0" stop-color="#f3e3a6" />
                    <stop offset="0.6" stop-color="#d4af37" />
                    <stop offset="1" stop-color="#7a5c14" />
                </linearGradient>
            </defs>
            <path d="M20 100 A80 80 0 0 1 180 100" fill="none" stroke="#8a6a1f" stroke-width="22" />
            <path d="M20 100 A80 80 0 0 1 180 100" fill="none" stroke="url(#vsgMeterScale)" stroke-width="18" />
            <g stroke="#fff" stroke-width="2">
                <line v-for="i in 7" :key="i" :x1="100 + 72 * Math.cos(Math.PI - (i * Math.PI) / 8)" :y1="100 - 72 * Math.sin(Math.PI - (i * Math.PI) / 8)" :x2="100 + 88 * Math.cos(Math.PI - (i * Math.PI) / 8)" :y2="100 - 88 * Math.sin(Math.PI - (i * Math.PI) / 8)" />
            </g>
            <text x="8" y="124" font-size="10" font-family="Cinzel, Georgia, serif" fill="#5c4a26">{{ t("meterLow") }}</text>
            <text x="194" y="124" font-size="10" font-family="Cinzel, Georgia, serif" font-weight="700" fill="#5c4a26" text-anchor="end">{{ t("meterHigh") }}</text>
            <g :style="{ transform: `rotate(${angle}deg)`, transformOrigin: '100px 100px', transition: 'transform 0.8s cubic-bezier(.3,1.6,.5,1)' }">
                <path d="M97 100 L100 26 L103 100 Z" fill="#2a2214" />
            </g>
            <circle cx="100" cy="100" r="9" fill="#c9a227" stroke="#8a6a1f" stroke-width="2" />
            <circle cx="100" cy="100" r="3" fill="#fff3c4" />
        </svg>
        <div class="vsg-meter-label">{{ t("meterLabel") }}<span v-if="overflow" class="vsg-blink"> {{ t("meterOverflow") }}</span></div>
    </div>
</template>

<style scoped>
.vsg-meter {
    width: 150px;
    text-align: center;
}
.vsg-meter-label {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 0.66rem;
    color: var(--vsg-ink-soft);
    line-height: 1.1;
}
.is-overflow svg {
    animation: vsg-shake 0.3s linear infinite;
}
@keyframes vsg-shake {
    25% {
        transform: translate(1px, -1px);
    }
    75% {
        transform: translate(-1px, 1px);
    }
}
</style>
