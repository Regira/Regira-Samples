<script setup lang="ts">
import { onBeforeUnmount, ref } from "vue"
import { useRouter } from "vue-router"
import { useAppFeedback } from "@regira/modules/vue/ui"
import GameLayout from "../components/GameLayout.vue"
import Chandelier from "../components/Chandelier.vue"
import useGameStore from "../store"
import { sounds } from "../effects"
import { useScreenTexts } from "../texts"
import { friendlyMessage } from "../errors"

const router = useRouter()
const store = useGameStore()
const feedback = useAppFeedback()

const texts = useScreenTexts()
const { t } = texts
const honorifics = texts.all("honorific")
const name = ref("")
const honorific = ref(honorifics[0] ?? "")
const startLabel = t("startLabel")
const testimonials = texts.rows("testimonial").map((t) => ({ quote: t.text, who: t.detail ?? texts.t("anonymousAuthor") }))

// Calibration theatre: the genius scale always overflows.
const calibrating = ref(false)
const gauge = ref(0)
let timer: ReturnType<typeof setInterval> | undefined

async function start() {
    calibrating.value = true
    gauge.value = 0
    if (store.soundOn) sounds.boing()
    timer = setInterval(() => (gauge.value = Math.min(gauge.value + 7 + Math.random() * 9, 187)), 90)
    try {
        const [game] = await Promise.all([store.start(name.value.trim(), honorific.value), new Promise((r) => setTimeout(r, 2200))])
        await router.push({ name: "play", params: { key: game.key } })
    } catch (ex) {
        console.error(ex)
        feedback.fail(friendlyMessage(ex) ?? t("startFailed"))
    } finally {
        clearInterval(timer)
        calibrating.value = false
    }
}
onBeforeUnmount(() => clearInterval(timer))
</script>

<template>
    <GameLayout>
        <Chandelier />
        <h1 class="vsg-wordart mt-2">{{ t("landingTitle") }}</h1>
        <p class="vsg-subtitle">{{ t("landingSubtitle") }}</p>
        <div class="vsg-divider" aria-hidden="true"></div>

        <section class="vsg-panel text-center mt-4">
            <div class="vsg-cherubs" aria-hidden="true"><span class="vsg-gild">👼</span><span class="vsg-gild vsg-mirror">👼</span></div>
            <h2 class="vsg-question-title mb-1">{{ t("landingQuestion") }}</h2>
            <p class="vsg-yes mb-3"><span class="vsg-blink">{{ t("landingAnswer") }}</span></p>

            <form class="mx-auto" style="max-width: 520px" @submit.prevent="start">
                <label for="playerName" class="form-label vsg-label">{{ t("nameLabel") }}</label>
                <input
                    id="playerName"
                    v-model="name"
                    maxlength="64"
                    autocomplete="nickname"
                    class="form-control form-control-lg text-center vsg-input mb-3"
                    :placeholder="t('namePlaceholder')"
                />
                <div class="d-flex flex-wrap justify-content-center gap-2 mb-4" role="radiogroup" :aria-label="t('honorificAria')">
                    <button
                        v-for="h in honorifics"
                        :key="h"
                        type="button"
                        role="radio"
                        :aria-checked="honorific === h"
                        class="vsg-chip"
                        :class="{ 'is-active': honorific === h }"
                        @click="honorific = h"
                    >
                        {{ h }}
                    </button>
                </div>
                <button type="submit" class="vsg-btn vsg-btn-huge" :disabled="calibrating">{{ startLabel }}</button>
            </form>

            <p v-if="store.lastGameKey" class="mt-3 mb-0">
                <RouterLink :to="{ name: 'play', params: { key: store.lastGameKey } }" class="vsg-continue">{{ t("continueLink") }}</RouterLink>
            </p>
        </section>

        <section class="row g-4">
            <div v-for="t in testimonials" :key="t.who" class="col-sm-6">
                <blockquote class="vsg-testimonial h-100 mb-0">
                    <p class="mb-1">“{{ t.quote }}”</p>
                    <footer>— {{ t.who }} <span class="vsg-gild">⭐⭐⭐⭐⭐</span></footer>
                </blockquote>
            </div>
        </section>

        <!-- calibration overlay -->
        <div v-if="calibrating" class="vsg-overlay" role="status" aria-live="polite">
            <div class="vsg-panel text-center vsg-calibrate">
                <div class="fs-1 vsg-gild vsg-wobble mt-2">🧠</div>
                <h2 class="vsg-question-title">{{ t("calibrating") }}</h2>
                <div class="vsg-gauge my-3"><div class="vsg-gauge-bar" :style="{ width: Math.min(gauge, 100) + '%' }"></div></div>
                <div class="vsg-gauge-value">{{ Math.round(gauge) }}%</div>
                <div v-if="gauge > 100" class="vsg-exceeded vsg-blink">{{ t("scaleExceeded") }}</div>
            </div>
        </div>
    </GameLayout>
</template>

<style scoped lang="scss">
.vsg-cherubs {
    display: flex;
    justify-content: space-between;
    font-size: 2.2rem;
    margin: -0.5rem 0.5rem -2.2rem;
}
.vsg-mirror {
    transform: scaleX(-1);
}
.vsg-question-title {
    font-family: var(--vsg-caps);
    font-weight: 900;
    color: var(--vsg-ink);
    font-size: clamp(1.5rem, 5vw, 2.3rem);
}
.vsg-yes {
    font-family: var(--vsg-script);
    font-size: 2.2rem;
    color: var(--vsg-gold-deep);
}
.vsg-label {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 1.05rem;
}
.vsg-input {
    font-family: var(--vsg-serif);
    border: 2px solid var(--vsg-gold);
    outline: 1px solid rgba(201, 162, 39, 0.55);
    outline-offset: 3px;
    border-radius: 3px;
    background: #fff;
}
.vsg-chip {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 0.85rem;
    border: 1px solid var(--vsg-gold);
    background: #fff;
    color: var(--vsg-ink);
    border-radius: 999px;
    padding: 0.35rem 1rem;
    &.is-active {
        background: var(--vsg-metal);
        border-color: var(--vsg-gold-deep);
        box-shadow: 0 0 0 2px #fff, 0 0 0 3px var(--vsg-gold-deep);
    }
}
.vsg-continue {
    font-family: var(--vsg-caps);
    font-weight: 700;
    color: var(--vsg-gold-deep);
}
.vsg-testimonial {
    background: #fff;
    border: 1px solid var(--vsg-gold);
    outline: 1px solid var(--vsg-gold);
    outline-offset: 4px;
    padding: 1rem 1.25rem;
    font-style: italic;
    font-size: 1.05rem;
    footer {
        font-style: normal;
        font-family: var(--vsg-caps);
        font-size: 0.8rem;
        color: var(--vsg-ink-soft);
    }
}
.vsg-overlay {
    position: fixed;
    inset: 0;
    z-index: 2000;
    background: rgba(255, 253, 247, 0.93);
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 16px;
}
.vsg-calibrate {
    width: min(480px, 100%);
}
.vsg-gauge {
    height: 22px;
    border: 2px solid var(--vsg-gold-deep);
    outline: 1px solid var(--vsg-gold);
    outline-offset: 3px;
    background: #fff;
    overflow: hidden;
}
.vsg-gauge-bar {
    height: 100%;
    background: var(--vsg-metal);
    transition: width 0.09s linear;
}
.vsg-gauge-value {
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: 2rem;
}
.vsg-exceeded {
    font-family: var(--vsg-caps);
    font-weight: 900;
    letter-spacing: 0.1em;
    text-transform: uppercase;
    color: var(--vsg-gold-deep);
}
</style>
