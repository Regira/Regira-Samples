<script setup lang="ts">
import { ref } from "vue"
import { injectModal, useFeedback, Feedback } from "@regira/modules/vue/ui"
import type { RevealResult } from "../api"
import { useScreenTexts } from "../texts"
import { friendlyMessage } from "../errors"

// Showing the real answer is possible - but only for those who REALLY want it. Two hurdles, then the facts,
// in the plainest, most tasteful modal on the site (the library's own DefaultModal).
const props = defineProps<{ reveal: () => Promise<RevealResult> }>()
const emit = defineEmits<{ close: [] }>()

const Modal = injectModal()
const feedback = useFeedback()
const { t } = useScreenTexts()
const ask1 = t("revealAsk1")
const ask2 = t("revealAsk2")
const keep = [t("revealKeepLabel"), t("revealKeepLabel")]
const backLabel = t("revealBackLabel")
const step = ref(1)
const result = ref<RevealResult>()
const dodged = ref(false)
const dodgeStyle = ref<Record<string, string>>({})

// the "show anyway" button runs away once
function dodge() {
    if (dodged.value) return
    dodged.value = true
    dodgeStyle.value = { transform: `translate(${Math.random() < 0.5 ? -120 : 120}px, ${-40 - Math.random() * 30}px)` }
}

async function show() {
    feedback.pending(t("revealPending"))
    try {
        result.value = await props.reveal()
        feedback.reset()
        step.value = 3
    } catch (ex) {
        console.error(ex)
        feedback.fail(friendlyMessage(ex) ?? t("revealFailed"))
    }
}
</script>

<template>
    <!-- DefaultModal renders in place: teleport it out of the game page (whose effects could clip it) -->
    <Teleport to="#modals">
        <component :is="Modal" :is-visible="true" :title="step < 3 ? t('revealTitle') : t('revealResultTitle')" :show-footer="false" size="md" @close="emit('close')" @cancel="emit('close')">
            <div class="vsg-gauntlet">
                <template v-if="step === 1">
                    <p>{{ ask1 }}</p>
                    <div class="d-flex flex-wrap gap-2 justify-content-between align-items-center">
                        <button type="button" class="btn btn-warning btn-lg fw-bold" @click="emit('close')">{{ keep[0] }}</button>
                        <button type="button" class="btn btn-link btn-sm text-muted" @click="step = 2">{{ t("revealYes") }}</button>
                    </div>
                </template>

                <template v-else-if="step === 2">
                    <p>{{ ask2 }}</p>
                    <Feedback :feedback="feedback" />
                    <div class="d-flex flex-wrap gap-2 justify-content-between align-items-center">
                        <button type="button" class="btn btn-warning btn-lg fw-bold" @click="emit('close')">{{ keep[1] }}</button>
                        <button
                            type="button"
                            class="btn btn-link btn-sm text-muted vsg-dodge"
                            :style="dodgeStyle"
                            :disabled="feedback.isPending"
                            @pointerenter="dodge"
                            @focus="dodge"
                            @click="show"
                        >
                            {{ t("revealAnyway") }}
                        </button>
                    </div>
                </template>

                <template v-else-if="result">
                    <p class="small text-muted mb-1">{{ t("revealAccording") }}</p>
                    <p class="fs-5 mb-2">{{ result.officialAnswer }}</p>
                    <p v-if="result.note" class="small text-muted fst-italic">{{ result.note }}</p>
                    <div class="alert alert-warning mb-3">
                        <strong v-if="result.youWereRight">{{ t("revealRight") }} </strong>
                        {{ result.praise }}
                        <span v-if="result.points" class="d-block fw-bold mt-1">{{ t("revealCourage", { points: result.points }) }}</span>
                    </div>
                    <button type="button" class="btn btn-warning fw-bold w-100" @click="emit('close')">{{ backLabel }}</button>
                </template>
            </div>
        </component>
    </Teleport>
</template>

<style scoped>
.vsg-gauntlet {
    font-family: Helvetica, Arial, sans-serif;
}
.vsg-dodge {
    transition: transform 0.25s ease-out;
}
</style>
