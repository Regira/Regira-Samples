<script setup lang="ts">
import { computed } from "vue"

// A gold laurel wreath (open at the top) framing whatever sits in the slot.
const props = withDefaults(defineProps<{ size?: number }>(), { size: 220 })

const leaves = computed(() => {
    const cx = 100
    const cy = 100
    const r = 82
    const out: Array<{ x: number; y: number; rot: number }> = []
    // left branch from the bottom (100°) up to the top-left (235°); right branch mirrored
    for (let i = 0; i < 11; i++) {
        const deg = 100 + i * 13.5
        const a = (deg * Math.PI) / 180
        const x = cx + r * Math.cos(a)
        const y = cy + r * Math.sin(a)
        const tangent = deg + 90
        out.push({ x, y, rot: tangent - 35 }, { x, y, rot: tangent + 35 })
        out.push({ x: 2 * cx - x, y, rot: 180 - (tangent - 35) }, { x: 2 * cx - x, y, rot: 180 - (tangent + 35) })
    }
    return out
})
const style = computed(() => ({ width: `${props.size}px`, height: `${props.size}px` }))
</script>

<template>
    <div class="vsg-laurel" :style="style">
        <svg viewBox="0 0 200 200" aria-hidden="true">
            <defs>
                <linearGradient id="vsgLaurelGold" x1="0" y1="0" x2="1" y2="1">
                    <stop offset="0" stop-color="#8a6a1f" />
                    <stop offset=".4" stop-color="#e9c85a" />
                    <stop offset=".55" stop-color="#fff3c4" />
                    <stop offset="1" stop-color="#9c7a22" />
                </linearGradient>
            </defs>
            <path d="M100 182 A82 82 0 0 1 34 52" fill="none" stroke="url(#vsgLaurelGold)" stroke-width="2.5" />
            <path d="M100 182 A82 82 0 0 0 166 52" fill="none" stroke="url(#vsgLaurelGold)" stroke-width="2.5" />
            <ellipse
                v-for="(l, i) in leaves"
                :key="i"
                :cx="l.x"
                :cy="l.y"
                rx="11"
                ry="4.2"
                fill="url(#vsgLaurelGold)"
                stroke="#8a6a1f"
                stroke-width=".5"
                :transform="`rotate(${l.rot} ${l.x} ${l.y}) translate(9 0)`"
            />
            <path d="M88 186 Q100 176 112 186 L106 196 L100 190 L94 196 Z" fill="url(#vsgLaurelGold)" />
        </svg>
        <div class="vsg-laurel-content"><slot /></div>
    </div>
</template>

<style scoped>
.vsg-laurel {
    position: relative;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    max-width: 100%;
}
.vsg-laurel svg {
    position: absolute;
    inset: 0;
    width: 100%;
    height: 100%;
}
.vsg-laurel-content {
    position: relative;
    text-align: center;
}
</style>
