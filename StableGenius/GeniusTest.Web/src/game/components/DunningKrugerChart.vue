<script setup lang="ts">
// The real Dunning-Kruger curve (confidence vs. competence) - with its first peak relabelled.
import { useScreenTexts } from "../texts"

defineProps<{ name: string }>()
const { t } = useScreenTexts()
</script>

<template>
    <figure class="vsg-dk mb-0">
        <svg viewBox="0 0 400 230" width="100%" role="img" aria-labelledby="vsgDkTitle vsgDkDesc">
            <title id="vsgDkTitle">{{ t("chartTitle") }}</title>
            <desc id="vsgDkDesc">{{ t("chartDescription") }}</desc>
            <defs>
                <linearGradient id="vsgDkGold" x1="0" x2="1">
                    <stop offset="0" stop-color="#8a6a1f" />
                    <stop offset=".25" stop-color="#e9c85a" />
                    <stop offset=".5" stop-color="#c9a227" />
                    <stop offset="1" stop-color="#8a6a1f" />
                </linearGradient>
            </defs>
            <!-- axes -->
            <line x1="40" y1="195" x2="385" y2="195" stroke="#5c4a26" stroke-width="1.5" />
            <line x1="40" y1="195" x2="40" y2="20" stroke="#5c4a26" stroke-width="1.5" />
            <text x="212" y="220" text-anchor="middle" font-size="12" font-family="Cinzel, Georgia, serif" fill="#5c4a26">{{ t("chartKnowledge") }}</text>
            <text x="16" y="110" text-anchor="middle" font-size="12" font-family="Cinzel, Georgia, serif" fill="#5c4a26" transform="rotate(-90 16 110)">
                {{ t("chartConfidence") }}
            </text>
            <!-- the curve -->
            <path d="M45 190 C 70 40, 95 30, 115 45 S 160 175, 205 170 S 300 110, 380 85" fill="none" stroke="url(#vsgDkGold)" stroke-width="6" stroke-linecap="round" />
            <!-- labels -->
            <text x="232" y="160" font-size="11" font-style="italic" font-family="Playfair Display, Georgia, serif" fill="#5c4a26">{{ t("chartValley") }}</text>
            <text x="296" y="74" font-size="11" font-style="italic" font-family="Playfair Display, Georgia, serif" fill="#5c4a26">{{ t("chartProfessors") }}</text>
            <!-- you -->
            <g class="vsg-dk-you">
                <circle cx="100" cy="37" r="10" fill="url(#vsgDkGold)" stroke="#2a2214" stroke-width="1.5" />
                <text x="100" y="41" text-anchor="middle" font-size="10">👑</text>
            </g>
            <text x="128" y="22" font-size="14" font-weight="700" font-family="Cinzel, Georgia, serif" fill="#8a6a1f">{{ t("chartSummit") }}</text>
            <text x="128" y="39" font-size="12" font-style="italic" font-family="Playfair Display, Georgia, serif" fill="#2a2214">{{ t("chartYou", { name }) }}</text>
        </svg>
        <figcaption class="text-center fst-italic mt-1">{{ t("chartCaption") }}</figcaption>
    </figure>
</template>

<style scoped>
.vsg-dk-you {
    transform-origin: 100px 37px;
    animation: vsg-bounce 1.6s ease-in-out infinite;
}
@keyframes vsg-bounce {
    50% {
        transform: translateY(-4px) scale(1.12);
    }
}
</style>
