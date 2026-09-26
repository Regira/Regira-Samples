<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref } from "vue"
import type { AnswerResult } from "../api"
import { reducedMotion } from "../effects"
import { useScreenTexts } from "../texts"
import Laurel from "./Laurel.vue"

const props = defineProps<{ reaction: AnswerResult; busy?: boolean }>()
const emit = defineEmits<{ next: []; reveal: [] }>()

const { t } = useScreenTexts()
const nextLabel = props.reaction.isLast ? t("resultsLabel") : t("nextLabel")

// count the points up, slot-machine style
const shownPoints = ref(reducedMotion() ? props.reaction.points : 0)
let raf = 0
onMounted(() => {
    if (reducedMotion()) return
    const startedAt = performance.now()
    const tick = (now: number) => {
        const t = Math.min((now - startedAt) / 1200, 1)
        shownPoints.value = Math.round(props.reaction.points * (1 - Math.pow(1 - t, 3)))
        if (t < 1) raf = requestAnimationFrame(tick)
    }
    raf = requestAnimationFrame(tick)
})
onBeforeUnmount(() => cancelAnimationFrame(raf))
</script>

<template>
    <div class="vsg-reaction-backdrop" role="dialog" aria-modal="true" aria-labelledby="vsgReactionHeadline">
        <div class="vsg-reaction vsg-panel" :class="{ 'is-legendary': reaction.legendary }">
            <div class="vsg-chyron"><span class="vsg-chyron-live">{{ t("liveBadge") }}</span> {{ t("chyron") }}</div>

            <h2 id="vsgReactionHeadline" class="vsg-headline">{{ reaction.headline }}</h2>
            <p class="vsg-body">{{ reaction.body }}</p>
            <p v-if="reaction.closer" class="vsg-closer">{{ reaction.closer }}</p>

            <Laurel :size="170" class="my-1">
                <div class="vsg-medal">
                    <div class="vsg-points-value">+{{ shownPoints.toLocaleString() }}</div>
                    <div class="vsg-points-label">{{ reaction.bonusLabel || t("pointsLabel") }}</div>
                </div>
            </Laurel>

            <div v-if="reaction.title && reaction.previousTitle !== reaction.title" class="vsg-promotion">
                <span class="vsg-gild">🎖️</span> {{ t("promoted") }} <s>{{ reaction.previousTitle }}</s> → <strong>{{ reaction.title }}</strong>
            </div>

            <button type="button" class="vsg-btn vsg-btn-huge mt-4" :disabled="busy" autofocus @click="emit('next')">{{ nextLabel }}</button>

            <div class="mt-3">
                <span v-if="reaction.revealed" class="text-muted small fst-italic">{{ t("peeked") }}</span>
                <button v-else type="button" class="vsg-boring-link" @click="emit('reveal')">{{ t("revealLink") }}</button>
            </div>
        </div>
    </div>
</template>

<style scoped lang="scss">
.vsg-reaction-backdrop {
    position: fixed;
    inset: 0;
    z-index: 1500;
    background: rgba(255, 253, 247, 0.9);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 40px 16px 16px;
    overflow-y: auto;
    // gold sunburst behind the card
    &::before {
        content: "";
        position: fixed;
        inset: -50vmax;
        background: repeating-conic-gradient(from 0deg, rgba(212, 175, 55, 0.3) 0 7deg, transparent 7deg 14deg);
        animation: vsg-rays 60s linear infinite;
        pointer-events: none;
    }
}
@keyframes vsg-rays {
    to {
        transform: rotate(360deg);
    }
}
.vsg-reaction {
    width: min(620px, 100%);
    text-align: center;
    margin: auto;
    padding-top: 0;
    animation: vsg-pop 0.45s cubic-bezier(0.3, 1.6, 0.5, 1);
    &.is-legendary {
        background-color: #fffaf0;
    }
}
@keyframes vsg-pop {
    from {
        transform: scale(0.3);
        opacity: 0;
    }
}
.vsg-chyron {
    margin: 0.9rem auto 1.2rem;
    display: inline-block;
    background: var(--vsg-ink);
    color: #e9c85a;
    font-family: var(--vsg-caps);
    font-weight: 700;
    letter-spacing: 0.14em;
    text-transform: uppercase;
    font-size: 0.8rem;
    padding: 0.3rem 1rem;
    border: 1px solid var(--vsg-gold);
    outline: 1px solid var(--vsg-gold);
    outline-offset: 2px;
}
.vsg-chyron-live {
    color: #fff;
    margin-right: 0.4rem;
    animation: vsg-blink 1.2s steps(2, start) infinite;
}
.vsg-headline {
    font-family: var(--vsg-display);
    font-weight: 900;
    font-size: clamp(1.6rem, 5.5vw, 2.4rem);
    background: var(--vsg-gold-text);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    filter: drop-shadow(0 1px 0 rgba(138, 106, 31, 0.5));
}
.vsg-body {
    font-size: clamp(1.1rem, 3.8vw, 1.4rem);
    font-weight: 700;
    color: var(--vsg-ink);
}
.vsg-closer {
    font-family: var(--vsg-script);
    font-size: 1.8rem;
    color: var(--vsg-gold-deep);
    line-height: 1.2;
}
.vsg-medal {
    width: 112px;
    height: 112px;
    border-radius: 50%;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    background:
        radial-gradient(circle, transparent 62%, rgba(138, 106, 31, 0.45) 63%, transparent 66%),
        var(--vsg-metal);
    box-shadow:
        0 0 0 3px #fff,
        0 0 0 5px var(--vsg-gold-deep),
        0 8px 16px rgba(90, 70, 20, 0.35);
}
.vsg-points-value {
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: 1.6rem;
    color: var(--vsg-ink);
    line-height: 1;
    text-shadow: 0 1px 0 rgba(255, 255, 255, 0.7);
}
.vsg-points-label {
    font-family: var(--vsg-caps);
    font-size: 0.55rem;
    font-weight: 700;
    color: var(--vsg-ink);
    max-width: 90px;
    line-height: 1.1;
    margin-top: 0.2rem;
}
.vsg-promotion {
    margin-top: 0.75rem;
    font-size: 1.05rem;
    border-top: 1px solid var(--vsg-gold);
    border-bottom: 1px solid var(--vsg-gold);
    padding: 0.4rem;
}
</style>
