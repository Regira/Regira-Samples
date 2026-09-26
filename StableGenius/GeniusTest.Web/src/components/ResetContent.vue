<script setup lang="ts">
import { computed, reactive, ref } from "vue"
import { useAxios } from "@regira/modules/vue/http"
import { Feedback, injectModal, toFeedbackError, useFeedback } from "@regira/modules/vue/ui"
import { defaultPoolCache } from "@regira/modules/vue/entities"
import { useLang } from "@regira/modules/vue/lang"
import { Entity as Question } from "@/entities/questions"
import { Entity as Reaction } from "@/entities/reactions"

// The big red button: resets all data - but only after answering 3 random quiz questions correctly.
// Games are untouched server-side (each plays from its own question snapshot), so nobody mid-game notices.
interface Challenge {
    questionId: number
    title: string
    emoji?: string
    type: "Choice" | "Number"
    unit?: string
    options: Array<{ id: number; text: string }>
}

const Modal = injectModal()
const feedback = useFeedback({ autoHideDelay: 15000 })
const dialogFeedback = useFeedback({ autoHideDelay: 0 })
const { translate } = useLang()

const open = ref(false)
// undefined = loading; [] = nothing to ask (the reset is free)
const challenges = ref<Array<Challenge>>()
const answers = reactive<Record<number, { optionId?: number; number?: number }>>({})
const isAnswered = (c: Challenge) => {
    const a = answers[c.questionId]
    return c.type === "Choice" ? a?.optionId != null : a?.number != null && !Number.isNaN(a.number)
}
const canSubmit = computed(() => challenges.value != null && challenges.value.every(isAnswered))
// one question at a time; the answers are checked together at the end (a mistake doesn't say which one)
const step = ref(0)
const current = computed(() => challenges.value?.[step.value])
const isLast = computed(() => challenges.value == null || step.value >= challenges.value.length - 1)
const canContinue = computed(() => (isLast.value ? canSubmit.value : current.value != null && isAnswered(current.value)))
function next() {
    if (!canContinue.value) return
    if (isLast.value) void reset()
    else step.value++
}

async function loadChallenge() {
    challenges.value = undefined
    step.value = 0
    for (const key of Object.keys(answers)) delete answers[+key]
    const { data } = await useAxios().get<Array<Challenge>>("/content/reset-challenge")
    for (const c of data) answers[c.questionId] = {}
    challenges.value = data
}

async function start() {
    feedback.reset()
    dialogFeedback.reset()
    open.value = true
    try {
        await loadChallenge()
    } catch (ex) {
        console.error(ex)
        dialogFeedback.fail(translate("resetFailed"), toFeedbackError(ex))
    }
}

function close() {
    open.value = false
}

async function reset() {
    dialogFeedback.pending(translate("resetPending"))
    const request = { answers: (challenges.value ?? []).map((c) => ({ questionId: c.questionId, ...answers[c.questionId] })) }
    try {
        await useAxios().post("/content/reset", request)
        // the reseeded rows get new ids: drop the pooled copies so no admin list shows a stale row
        defaultPoolCache.getEntityMap(Question.name).clear()
        defaultPoolCache.getEntityMap(Reaction.name).clear()
        close()
        feedback.success(translate("resetDone"))
    } catch (ex) {
        if ((ex as { response?: { status?: number } }).response?.status === 422) {
            // a wrong answer: nothing happened - and no second try at the same questions
            dialogFeedback.fail(translate("resetWrong"))
            await loadChallenge()
            return
        }
        console.error(ex)
        dialogFeedback.fail(translate("resetFailed"), toFeedbackError(ex))
    }
}
</script>

<template>
    <section class="reset-content text-center">
        <h2 class="admin-title h3 mb-2">{{ $t("resetTitle") }}</h2>
        <p class="mb-3">{{ $t("resetIntro") }}</p>

        <button type="button" class="big-red-button" :disabled="open" @click="start">
            <span class="big-red-button__label">{{ $t("resetButton") }}</span>
        </button>

        <div class="reset-feedback mx-auto mt-3"><Feedback :feedback="feedback" /></div>

        <!-- like the library's own popups: DefaultModal renders in place, so teleport it into the #modals host -->
        <Teleport to="#modals">
            <component :is="Modal" v-if="open" :is-visible="true" :title="$t('resetConfirmTitle')" :show-footer="false" size="md" @close="close" @cancel="close">
                <div class="reset-challenge text-start">
                    <p class="mb-3">{{ $t(challenges?.length === 0 ? "resetNoQuestion" : "resetConfirmBody", { count: challenges?.length ?? 3 }) }}</p>

                    <p v-if="challenges === undefined" class="text-muted small">{{ $t("resetLoadingQuestion") }}</p>
                    <div v-else-if="current" :key="current.questionId" class="reset-challenge__step mb-3">
                        <p class="small text-muted mb-1">{{ $t("resetProgress", { current: step + 1, total: challenges.length }) }}</p>
                        <p class="reset-challenge__question mb-2">
                            <span class="reset-challenge__number">{{ step + 1 }}</span>
                            <span v-if="current.emoji" class="me-1">{{ current.emoji }}</span>{{ current.title }}
                        </p>
                        <div v-if="current.type === 'Choice'" class="d-grid gap-1">
                            <template v-for="o in current.options" :key="o.id">
                                <input
                                    :id="`reset-${current.questionId}-${o.id}`"
                                    v-model="answers[current.questionId]!.optionId"
                                    type="radio"
                                    class="btn-check"
                                    :name="`reset-${current.questionId}`"
                                    :value="o.id"
                                />
                                <label :for="`reset-${current.questionId}-${o.id}`" class="btn btn-sm btn-outline-secondary text-start">{{ o.text }}</label>
                            </template>
                        </div>
                        <div v-else class="input-group input-group-sm">
                            <input
                                v-model.number="answers[current.questionId]!.number"
                                type="number"
                                step="any"
                                class="form-control"
                                :aria-label="$t('answer')"
                                @keyup.enter="next"
                            />
                            <span v-if="current.unit" class="input-group-text">{{ current.unit }}</span>
                        </div>
                    </div>

                    <Feedback :feedback="dialogFeedback" />
                    <div class="d-flex justify-content-between gap-2 mt-2">
                        <button type="button" class="btn btn-outline-secondary" @click="close">{{ $t("cancel") }}</button>
                        <button
                            type="button"
                            class="btn fw-bold"
                            :class="isLast ? 'btn-danger' : 'btn-outline-dark'"
                            :disabled="!canContinue || dialogFeedback.isPending"
                            @click="next"
                        >
                            {{ isLast ? $t("resetSubmit") : $t("resetNext") }}
                        </button>
                    </div>
                </div>
            </component>
        </Teleport>
    </section>
</template>

<style scoped>
.reset-content {
    margin: 2rem auto 1rem;
    padding: 1.5rem 1rem 1rem;
    max-width: 640px;
    border-top: 3px double #c9a227;
}
/* a round red button in a gold bezel: pressing it should feel irreversible (it is) */
.big-red-button {
    width: 9.5rem;
    height: 9.5rem;
    border-radius: 50%;
    border: 6px solid #c9a227;
    outline: 3px solid #8a6a1f;
    outline-offset: 3px;
    background: radial-gradient(circle at 35% 30%, #ff6b6b 0%, #d10000 45%, #7a0000 100%);
    color: #fff;
    box-shadow:
        0 10px 0 #5a0000,
        0 16px 28px rgba(90, 0, 0, 0.45),
        inset 0 -6px 12px rgba(0, 0, 0, 0.35);
    transition:
        transform 0.08s,
        box-shadow 0.08s;
}
.big-red-button:active {
    transform: translateY(8px);
    box-shadow:
        0 2px 0 #5a0000,
        0 6px 12px rgba(90, 0, 0, 0.45),
        inset 0 -4px 8px rgba(0, 0, 0, 0.35);
}
.big-red-button__label {
    font-family: "Cinzel", Georgia, serif;
    font-weight: 900;
    font-size: 1.6rem;
    letter-spacing: 0.08em;
    text-shadow: 0 2px 0 rgba(0, 0, 0, 0.4);
}
.reset-feedback {
    max-width: 560px;
}
.reset-challenge__number {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 1.5rem;
    height: 1.5rem;
    margin-right: 0.4rem;
    border-radius: 50%;
    background: #c9a227;
    color: #fff;
    font-size: 0.8rem;
}
.reset-challenge__question {
    font-family: "Cinzel", Georgia, serif;
    font-weight: 700;
    font-size: 1.05rem;
}
</style>
