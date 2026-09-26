<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue"
import { reducedMotion } from "../effects"
import type { AnswerRequest, PlayQuestion } from "../api"
import { useScreenTexts } from "../texts"

const props = defineProps<{ question: PlayQuestion; busy?: boolean }>()
const emit = defineEmits<{ answer: [AnswerRequest] }>()

const numerals = ["I", "II", "III", "IV", "V", "VI"]
const { t } = useScreenTexts()
const submitText = () => t("submitLabel")
const skipText = () => t("skipLabel")

const number = ref<number>()


// The unreachable option. It starts centred below the real answers and hops a short way whenever the
// pointer (almost) touches it - but only within its "stage": the middle strip below the answers.
// Touch and keyboard make it hop too. Only when there's no room nearby does it hop further (within the stage).
const panel = ref<HTMLElement>()
const runaway = ref<HTMLElement>()
// a function ref: a plain ref="…" inside v-for would collect an array
const setRunaway = (el: unknown) => (runaway.value = (el as HTMLElement | null) ?? undefined)
const dodgeOffset = ref({ x: 0, y: 0 })
const unreachableHint = ref<string>()
const FLEE_DISTANCE = 24 // px between pointer and button that triggers a hop
const MIN_JUMP = 70 // px per hop, at least...
const JUMP_REACH = [170, 240, 340] // ...and at most this far - the next reach is tried only when nothing fits
const STAGE_BAND = 0.6 // the middle 60% of the width below the answers

type Point = { x: number; y: number }
type Box = { left: number; top: number; right: number; bottom: number }
const distanceToBox = (p: Point, b: Box) => Math.hypot(Math.max(b.left - p.x, 0, p.x - b.right), Math.max(b.top - p.y, 0, p.y - b.bottom))
const overlaps = (a: Box, b: Box) => a.left < b.right && a.right > b.left && a.top < b.bottom && a.bottom > b.top
const grow = (r: DOMRect, by: number): Box => ({ left: r.left - by, top: r.top - by, right: r.right + by, bottom: r.bottom + by })

// Where the button is (or is gliding to), in client coordinates - from its layout slot plus the offset,
// so it's exact even mid-transition (a measured rect would still be on its way).
function buttonBox(): Box | undefined {
    const button = runaway.value
    const area = panel.value
    if (!button || !area) return
    const origin = area.getBoundingClientRect()
    const left = origin.left + area.clientLeft + button.offsetLeft + dodgeOffset.value.x
    const top = origin.top + area.clientTop + button.offsetTop + dodgeOffset.value.y
    return { left, top, right: left + button.offsetWidth, bottom: top + button.offsetHeight }
}

function dodge(pointer?: Point) {
    const button = runaway.value
    const area = panel.value
    const now = buttonBox()
    if (!button || !area || !now) return
    const stage = (button.parentElement ?? area).getBoundingClientRect()
    const width = button.offsetWidth
    const height = button.offsetHeight
    const home = { left: now.left - dodgeOffset.value.x, top: now.top - dodgeOffset.value.y }
    const current = { x: now.left + width / 2, y: now.top + height / 2 }
    const avoid = [...area.querySelectorAll(".vsg-option:not(.vsg-option--unreachable), .vsg-q-title, .vsg-unreachable-hint, .vsg-skip")]
        .map((el) => grow(el.getBoundingClientRect(), 10))
    const margin = (stage.width * (1 - STAGE_BAND)) / 2
    const bounds = {
        left: stage.left + margin,
        top: stage.top,
        right: stage.right - margin - width,
        bottom: stage.bottom - height,
    }
    const fits = (left: number, top: number) =>
        left >= bounds.left && left <= bounds.right && top >= bounds.top && top <= bounds.bottom &&
        !avoid.some((a) => overlaps({ left, top, right: left + width, bottom: top + height }, a)) &&
        (!pointer || Math.hypot(left + width / 2 - pointer.x, top + height / 2 - pointer.y) > FLEE_DISTANCE + Math.max(width, height))
    let best: { left: number; top: number } | undefined
    for (const reach of [...JUMP_REACH, Infinity]) {
        for (let i = 0; i < 80 && !best; i++) {
            // a hop in a random direction within reach; beyond the last reach, anywhere in the panel
            const angle = Math.random() * 2 * Math.PI
            const distance = MIN_JUMP + Math.random() * (reach - MIN_JUMP)
            const left = reach === Infinity ? bounds.left + Math.random() * (bounds.right - bounds.left) : current.x + Math.cos(angle) * distance - width / 2
            const top = reach === Infinity ? bounds.top + Math.random() * (bounds.bottom - bounds.top) : current.y + Math.sin(angle) * distance - height / 2
            if (fits(left, top)) best = { left, top }
        }
        if (best) break
    }
    if (!best) return
    dodgeOffset.value = { x: best.left - home.left, y: best.top - home.top }
    unreachableHint.value ??= t("unreachableHint")
}

// flee on approach: watch the pointer everywhere, not just on the button (checked at most every 25 ms)
let lastCheck = 0
function onPointerMove(e: PointerEvent) {
    if (!runaway.value || e.timeStamp - lastCheck < 25) return
    lastCheck = e.timeStamp
    const pointer = { x: e.clientX, y: e.clientY }
    const box = buttonBox()
    if (box && distanceToBox(pointer, box) < FLEE_DISTANCE) dodge(pointer)
}
const pointerOf = (e: Event): Point | undefined => ("clientX" in e ? { x: (e as PointerEvent).clientX, y: (e as PointerEvent).clientY } : undefined)
onMounted(() => window.addEventListener("pointermove", onPointerMove, { passive: true }))
onBeforeUnmount(() => window.removeEventListener("pointermove", onPointerMove))
const submitLabel = ref(submitText())
const skipLabel = ref(skipText())

watch(
    () => props.question.id,
    () => {
        number.value = undefined
        dodgeOffset.value = { x: 0, y: 0 }
        unreachableHint.value = undefined
        submitLabel.value = submitText()
        skipLabel.value = skipText()
    }
)

// The runaway option always goes last: centred below the real answers, on its own "stage".
const displayOptions = computed(() => [...props.question.options.filter((o) => !o.unreachable), ...props.question.options.filter((o) => o.unreachable)])

const ratingScale = computed(() => {
    const min = props.question.minValue ?? 1
    const max = props.question.maxValue ?? 10
    return Array.from({ length: Math.max(0, max - min + 1) }, (_, i) => min + i)
})

function bump(delta: number) {
    number.value = Math.round(((number.value ?? 0) + delta) * 100) / 100
}
function submitNumber() {
    if (number.value == null || Number.isNaN(number.value)) return
    emit("answer", { questionId: props.question.id, number: number.value })
}
</script>

<template>
    <section ref="panel" class="vsg-panel vsg-question">
        <div class="vsg-q-emoji vsg-gild vsg-wobble" aria-hidden="true">{{ question.emoji || "🤔" }}</div>
        <h2 class="vsg-q-title">{{ question.title }}</h2>
        <div class="vsg-divider" aria-hidden="true"></div>

        <!-- Choice: one tap answers. No wrong buttons exist. Answers + hint line are ONE branch:
             anything placed between a v-if and its v-else-if silently breaks the chain. -->
        <template v-if="question.type === 'Choice'">
            <div class="row g-3">
                <div v-for="(option, i) in displayOptions" :key="option.id" :class="option.unreachable ? 'col-12 vsg-runaway-stage' : 'col-12 col-sm-6'">
                    <!-- the one that got away: never emits an answer, whatever you try -->
                    <button
                        v-if="option.unreachable"
                        :ref="setRunaway"
                        type="button"
                        class="vsg-option vsg-option--unreachable"
                        :class="{ 'no-motion': reducedMotion() }"
                        :style="{ transform: `translate(${dodgeOffset.x}px, ${dodgeOffset.y}px)` }"
                        aria-disabled="true"
                        @pointerenter="dodge(pointerOf($event))"
                        @pointerdown.prevent="dodge(pointerOf($event))"
                        @focus="dodge()"
                        @click.prevent
                    >
                        <span class="vsg-numeral" aria-hidden="true">{{ numerals[i] }}</span>
                        <span class="vsg-option-text">{{ option.text }}</span>
                    </button>
                    <button v-else type="button" class="vsg-option" :disabled="busy" @click="emit('answer', { questionId: question.id, optionId: option.id })">
                        <span class="vsg-numeral" aria-hidden="true">{{ numerals[i] }}</span>
                        <span class="vsg-option-text">{{ option.text }}</span>
                    </button>
                </div>
            </div>
            <p v-if="unreachableHint" class="vsg-unreachable-hint mt-3 mb-0" role="status">{{ unreachableHint }}</p>
        </template>

        <!-- Rating: gold coins, 1-10 -->
        <div v-else-if="question.type === 'Rating'" class="vsg-rating">
            <button
                v-for="value in ratingScale"
                :key="value"
                type="button"
                class="vsg-coin"
                :disabled="busy"
                :aria-label="t('ratingAria', { value, max: question.maxValue ?? 10 })"
                @click="emit('answer', { questionId: question.id, number: value })"
            >
                {{ value }}
            </button>
        </div>

        <!-- Number: big field, big +/- buttons, no maths required -->
        <form v-else class="vsg-number" @submit.prevent="submitNumber">
            <div class="d-flex justify-content-center align-items-stretch gap-2 flex-wrap">
                <button type="button" class="vsg-bump" :disabled="busy" @click="bump(-10)">−10</button>
                <button type="button" class="vsg-bump" :disabled="busy" @click="bump(-1)">−1</button>
                <input
                    v-model.number="number"
                    type="number"
                    inputmode="decimal"
                    step="any"
                    class="vsg-number-input"
                    placeholder="?"
                    :aria-label="t('numberAria')"
                    :disabled="busy"
                />
                <button type="button" class="vsg-bump" :disabled="busy" @click="bump(1)">+1</button>
                <button type="button" class="vsg-bump" :disabled="busy" @click="bump(10)">+10</button>
            </div>
            <div v-if="question.unit" class="text-center fst-italic mt-1">({{ question.unit }})</div>
            <div class="text-center mt-4">
                <button type="submit" class="vsg-btn" :disabled="busy || number == null">{{ submitLabel }}</button>
            </div>
        </form>

        <div class="text-center mt-4">
            <button type="button" class="vsg-skip" :disabled="busy" @click="emit('answer', { questionId: question.id, skipped: true })">
                {{ skipLabel }}
            </button>
        </div>
    </section>
</template>

<style scoped lang="scss">
.vsg-question {
    text-align: center;
}
.vsg-q-emoji {
    font-size: clamp(3rem, 12vw, 4.5rem);
    line-height: 1;
    margin-top: 0.5rem;
}
.vsg-q-title {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: clamp(1.35rem, 4.6vw, 2.1rem);
    color: var(--vsg-ink);
    margin: 0.75rem 0 0;
}
.vsg-option {
    width: 100%;
    min-height: 4.2rem;
    display: flex;
    align-items: center;
    gap: 0.9rem;
    text-align: left;
    font-family: var(--vsg-serif);
    font-weight: 700;
    font-size: clamp(1.15rem, 4vw, 1.45rem);
    color: var(--vsg-ink);
    background: #fff;
    border: 2px solid var(--vsg-gold);
    outline: 1px solid rgba(201, 162, 39, 0.55);
    outline-offset: 3px;
    border-radius: 4px;
    padding: 0.6rem 1rem;
    box-shadow: 0 4px 10px rgba(90, 70, 20, 0.12);
    transition:
        background 0.15s,
        transform 0.1s;
    &:hover {
        background: linear-gradient(180deg, #fffaf0, #fff3c4);
        transform: translateY(-2px);
    }
    &:active {
        transform: translateY(2px);
    }
    &:disabled {
        opacity: 0.6;
    }
}
// small pill, not a full answer button - and quick on its feet
.vsg-option--unreachable {
    position: relative;
    z-index: 2;
    width: auto;
    min-height: 0;
    display: inline-flex;
    gap: 0.5rem;
    font-size: 0.95rem;
    padding: 0.3rem 0.9rem 0.3rem 0.35rem;
    border-radius: 999px;
    outline-offset: 2px;
    transition:
        transform 0.16s cubic-bezier(0.2, 1.3, 0.4, 1),
        background 0.15s;
    .vsg-numeral {
        width: 1.6rem;
        height: 1.6rem;
        font-size: 0.7rem;
    }
    &:hover {
        transform: none;
    }
    &.no-motion {
        transition: none;
    }
}
// room to hop around in, below the two real answers
.vsg-runaway-stage {
    min-height: 8.5rem;
    display: flex;
    align-items: flex-start;
    justify-content: center;
}
.vsg-unreachable-hint {
    font-style: italic;
    color: var(--vsg-ink-soft);
}
.vsg-numeral {
    flex: none;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    width: 2.4rem;
    height: 2.4rem;
    border-radius: 50%;
    font-family: var(--vsg-caps);
    font-size: 0.95rem;
    color: var(--vsg-ink);
    background: var(--vsg-metal);
    box-shadow:
        0 0 0 2px #fff,
        0 0 0 3px var(--vsg-gold-deep);
}
.vsg-rating {
    display: grid;
    grid-template-columns: repeat(5, 1fr);
    gap: 0.8rem;
    justify-items: center;
    @media (max-width: 420px) {
        grid-template-columns: repeat(4, 1fr);
    }
}
// gold coins with a milled edge
.vsg-coin {
    width: 4rem;
    height: 4rem;
    border-radius: 50%;
    font-family: var(--vsg-caps);
    font-weight: 900;
    font-size: 1.5rem;
    color: var(--vsg-ink);
    background:
        radial-gradient(circle, transparent 60%, rgba(138, 106, 31, 0.55) 61%, transparent 64%),
        var(--vsg-metal);
    border: 3px dotted var(--vsg-gold-deep);
    box-shadow: 0 5px 10px rgba(90, 70, 20, 0.3);
    text-shadow: 0 1px 0 rgba(255, 255, 255, 0.7);
    transition: transform 0.12s;
    &:hover {
        transform: translateY(-3px) rotate(-8deg) scale(1.08);
    }
}
.vsg-number-input {
    width: 9rem;
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 2.2rem;
    text-align: center;
    color: var(--vsg-ink);
    border: 2px solid var(--vsg-gold);
    outline: 1px solid var(--vsg-gold);
    outline-offset: 3px;
    border-radius: 4px;
    background: #fff;
}
.vsg-bump {
    font-family: var(--vsg-caps);
    font-weight: 700;
    font-size: 1.1rem;
    min-width: 3.4rem;
    border: 2px solid var(--vsg-gold);
    background: #fff;
    color: var(--vsg-ink);
    border-radius: 4px;
    &:hover {
        background: var(--vsg-metal);
    }
}
.vsg-skip {
    font-family: var(--vsg-serif);
    font-style: italic;
    background: none;
    border: none;
    border-bottom: 1px dotted var(--vsg-gold-deep);
    padding: 0 0.25rem;
    color: var(--vsg-ink-soft);
}
</style>
