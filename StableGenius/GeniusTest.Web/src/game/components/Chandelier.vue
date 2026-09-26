<script setup lang="ts">
import { useScreenTexts } from "../texts"

const { t } = useScreenTexts()
// A gilded crystal chandelier. Five arms, five candles, far too many crystals.
const arms = [
    { x: 22, y: 64 },
    { x: 62, y: 54 },
    { x: 110, y: 46 },
    { x: 158, y: 54 },
    { x: 198, y: 64 },
]
const crystals = [
    { x: 40, y: 92, d: 0 },
    { x: 72, y: 100, d: 0.4 },
    { x: 92, y: 108, d: 0.8 },
    { x: 110, y: 114, d: 0.2 },
    { x: 128, y: 108, d: 0.6 },
    { x: 148, y: 100, d: 1 },
    { x: 180, y: 92, d: 0.3 },
]
</script>

<template>
    <svg class="vsg-chandelier" viewBox="0 0 220 132" role="img" :aria-label="t('chandelierAria')">
        <defs>
            <linearGradient id="vsgChGold" x1="0" y1="0" x2="1" y2="1">
                <stop offset="0" stop-color="#8a6a1f" />
                <stop offset=".35" stop-color="#e9c85a" />
                <stop offset=".5" stop-color="#fff3c4" />
                <stop offset=".7" stop-color="#c9a227" />
                <stop offset="1" stop-color="#8a6a1f" />
            </linearGradient>
            <linearGradient id="vsgChCrystal" x1="0" y1="0" x2="1" y2="1">
                <stop offset="0" stop-color="#ffffff" />
                <stop offset=".5" stop-color="#e9eef3" />
                <stop offset="1" stop-color="#b9c4cf" />
            </linearGradient>
        </defs>
        <!-- chain + crown -->
        <line x1="110" y1="0" x2="110" y2="22" stroke="url(#vsgChGold)" stroke-width="3" stroke-dasharray="4 2" />
        <path d="M96 30 L100 20 L105 27 L110 17 L115 27 L120 20 L124 30 Z" fill="url(#vsgChGold)" />
        <!-- arms -->
        <g fill="none" stroke="url(#vsgChGold)" stroke-width="3.5" stroke-linecap="round">
            <path v-for="(a, i) in arms" :key="'arm' + i" :d="`M110 72 C ${110 + (a.x - 110) * 0.3} 96, ${a.x} 92, ${a.x} ${a.y + 8}`" />
        </g>
        <!-- body -->
        <ellipse cx="110" cy="44" rx="10" ry="16" fill="url(#vsgChGold)" />
        <ellipse cx="110" cy="74" rx="22" ry="9" fill="url(#vsgChGold)" />
        <circle cx="110" cy="88" r="5" fill="url(#vsgChGold)" />
        <!-- cups, candles, flames -->
        <g v-for="(a, i) in arms" :key="'c' + i">
            <path :d="`M${a.x - 9} ${a.y + 8} Q${a.x} ${a.y + 16} ${a.x + 9} ${a.y + 8} Z`" fill="url(#vsgChGold)" />
            <rect :x="a.x - 3.5" :y="a.y - 12" width="7" height="20" rx="1.5" fill="#fffdf5" stroke="#c9a227" stroke-width="1" />
            <path class="vsg-flame" :style="{ animationDelay: `${i * 0.23}s` }" :d="`M${a.x} ${a.y - 24} C${a.x + 5} ${a.y - 17} ${a.x + 3} ${a.y - 12} ${a.x} ${a.y - 12} C${a.x - 3} ${a.y - 12} ${a.x - 5} ${a.y - 17} ${a.x} ${a.y - 24} Z`" fill="#f0c040" />
        </g>
        <!-- crystal drops -->
        <g v-for="(c, i) in crystals" :key="'x' + i" class="vsg-crystal" :style="{ animationDelay: `${c.d}s`, transformOrigin: `${c.x}px ${c.y - 10}px` }">
            <line :x1="c.x" :y1="c.y - 12" :x2="c.x" :y2="c.y - 5" stroke="#c9a227" stroke-width="1" />
            <path :d="`M${c.x} ${c.y - 6} L${c.x + 4} ${c.y} L${c.x} ${c.y + 9} L${c.x - 4} ${c.y} Z`" fill="url(#vsgChCrystal)" stroke="#9aa7b4" stroke-width=".6" />
            <circle :cx="c.x - 1" :cy="c.y - 1" r="1" fill="#fff" />
        </g>
    </svg>
</template>

<style scoped>
.vsg-chandelier {
    display: block;
    width: min(220px, 60vw);
    margin: 0 auto;
    filter: drop-shadow(0 6px 10px rgba(138, 106, 31, 0.25));
}
.vsg-flame {
    transform-box: fill-box;
    transform-origin: 50% 100%;
    animation: vsg-flicker 1.1s ease-in-out infinite alternate;
}
@keyframes vsg-flicker {
    to {
        transform: scale(0.85, 1.12);
        opacity: 0.85;
    }
}
.vsg-crystal {
    animation: vsg-sway 3.2s ease-in-out infinite alternate;
}
@keyframes vsg-sway {
    from {
        transform: rotate(-4deg);
    }
    to {
        transform: rotate(4deg);
    }
}
</style>
