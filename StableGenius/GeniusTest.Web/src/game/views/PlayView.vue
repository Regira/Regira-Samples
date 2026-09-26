<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { useRouter } from "vue-router"
import { LoadingContainer, useAppFeedback } from "@regira/modules/vue/ui"
import GameLayout from "../components/GameLayout.vue"
import QuestionCard from "../components/QuestionCard.vue"
import ReactionOverlay from "../components/ReactionOverlay.vue"
import RevealGauntlet from "../components/RevealGauntlet.vue"
import GeniusMeter from "../components/GeniusMeter.vue"
import useGameStore from "../store"
import type { AnswerRequest, AnswerResult } from "../api"
import { celebrate, playFor } from "../effects"
import { friendlyMessage, toErrorPage } from "../errors"
import { useScreenTexts } from "../texts"

const props = defineProps<{ gameKey: string }>()
const router = useRouter()
const store = useGameStore()
const feedback = useAppFeedback()
const texts = useScreenTexts()

const loading = ref(true)
const busy = ref(false)
const reaction = ref<AnswerResult>()
const showGauntlet = ref(false)
const displayedScore = ref(0)

const game = computed(() => store.game)
const question = computed(() => store.currentQuestion)
const progress = computed(() => store.progress)

async function load() {
    loading.value = true
    try {
        const g = await store.load(props.gameKey)
        displayedScore.value = g.score
        if (g.isFinished || !store.currentQuestion) await goToResults()
    } catch (ex) {
        console.error(ex)
        await toErrorPage(router, ex)
    } finally {
        loading.value = false
    }
}
onMounted(load)
watch(() => props.gameKey, load)

// the score only ever counts UP
watch(
    () => game.value?.score ?? 0,
    (to) => {
        const from = displayedScore.value
        const startedAt = performance.now()
        const tick = (now: number) => {
            const t = Math.min((now - startedAt) / 900, 1)
            displayedScore.value = Math.round(from + (to - from) * t)
            if (t < 1) requestAnimationFrame(tick)
        }
        requestAnimationFrame(tick)
    }
)

async function answer(request: AnswerRequest) {
    if (busy.value) return
    busy.value = true
    try {
        reaction.value = await store.answer(request)
        celebrate(reaction.value.effect)
        if (store.soundOn) playFor(reaction.value.effect)
    } catch (ex) {
        console.error(ex)
        feedback.fail(friendlyMessage(ex) ?? texts.t("answerFailed"))
    } finally {
        busy.value = false
    }
}

async function next() {
    const wasLast = reaction.value?.isLast
    reaction.value = undefined
    if (wasLast || !store.currentQuestion) await goToResults()
    else window.scrollTo({ top: 0, behavior: "smooth" })
}

async function goToResults() {
    busy.value = true
    try {
        await store.finish(props.gameKey)
        await router.replace({ name: "results", params: { key: props.gameKey } })
    } catch (ex) {
        console.error(ex)
        feedback.fail(friendlyMessage(ex) ?? texts.t("finishFailed"))
    } finally {
        busy.value = false
    }
}

async function reveal() {
    const questionId = reaction.value!.questionId
    const result = await store.reveal(questionId)
    if (reaction.value) reaction.value = { ...reaction.value, revealed: true }
    return result
}
</script>

<template>
    <GameLayout>
        <LoadingContainer :is-loading="loading">
            <template v-if="game">
                <div class="vsg-hud vsg-panel">
                    <div class="vsg-hud-cell">
                        <div class="vsg-hud-label">{{ game.honorific }}</div>
                        <div class="vsg-hud-name text-truncate">{{ game.playerName }}</div>
                        <div class="vsg-hud-title"><span class="vsg-gild">👑</span> {{ game.title }}</div>
                    </div>
                    <div class="vsg-hud-cell text-center">
                        <div class="vsg-hud-label">{{ texts.t("scoreLabel") }}</div>
                        <div class="vsg-hud-score">{{ displayedScore.toLocaleString() }}</div>
                        <div class="vsg-stars" :aria-label="texts.t('progressAria', { current: progress.done + 1, total: progress.total })">
                            <span v-for="i in progress.total" :key="i" :class="{ 'vsg-gild': i <= progress.done }">{{ i <= progress.done ? "⭐" : "☆" }}</span>
                        </div>
                    </div>
                    <div class="vsg-hud-cell d-none d-sm-flex justify-content-end">
                        <GeniusMeter :level="game.level" :max="progress.total" />
                    </div>
                </div>

                <QuestionCard v-if="question" :key="question.id" :question="question" :busy="busy" @answer="answer" />
            </template>
        </LoadingContainer>

        <ReactionOverlay v-if="reaction" :key="reaction.questionId" :reaction="reaction" :busy="busy" @next="next" @reveal="showGauntlet = true" />
        <RevealGauntlet v-if="showGauntlet" :reveal="reveal" @close="showGauntlet = false" />
    </GameLayout>
</template>

<style scoped lang="scss">
.vsg-hud {
    display: grid;
    grid-template-columns: 1fr auto 1fr;
    align-items: center;
    gap: 0.75rem;
    padding: 1.4rem 1.5rem 1rem;
    margin-top: 1.5rem;
    @media (max-width: 575px) {
        grid-template-columns: 1fr auto;
    }
}
.vsg-hud-cell {
    min-width: 0;
}
.vsg-hud-label {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 0.7rem;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    color: var(--vsg-gold-deep);
}
.vsg-hud-name {
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: 1.2rem;
}
.vsg-hud-title {
    font-style: italic;
    color: var(--vsg-ink-soft);
}
.vsg-hud-score {
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: clamp(1.6rem, 6vw, 2.4rem);
    line-height: 1.1;
    background: var(--vsg-gold-text);
    -webkit-background-clip: text;
    background-clip: text;
    color: transparent;
    filter: drop-shadow(0 1px 0 rgba(138, 106, 31, 0.5));
}
.vsg-stars {
    font-size: 0.95rem;
    letter-spacing: -1px;
    color: var(--vsg-gold);
}
</style>
