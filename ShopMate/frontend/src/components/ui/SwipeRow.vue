<!--
    A touch-first row with swipe actions.
    - swipe RIGHT reveals the #start slot and emits "swipe-right" when released past the threshold
    - swipe LEFT reveals the #end slot and emits "swipe-left"
    Vertical scrolling keeps working (touch-action: pan-y); a small horizontal dead zone avoids accidental swipes.
    Every swipe action must also be reachable without swiping (a tap target or a menu) - swiping is a shortcut.
-->
<script setup lang="ts">
import { computed, ref } from "vue"

const props = withDefaults(defineProps<{ disabled?: boolean; threshold?: number }>(), { threshold: 88 })
const emit = defineEmits<{ (e: "swipe-left"): void; (e: "swipe-right"): void }>()
defineSlots<{ default?(): any; start?(props: { armed: boolean }): any; end?(props: { armed: boolean }): any }>()

const dx = ref(0)
const isSwiping = ref(false)
let startX = 0
let startY = 0
let pointerId: number | undefined
let locked: "x" | "y" | undefined
let suppressClick = false

const armedRight = computed(() => dx.value >= props.threshold)
const armedLeft = computed(() => dx.value <= -props.threshold)
const style = computed(() => ({
    transform: dx.value ? `translate3d(${dx.value}px, 0, 0)` : undefined,
    transition: isSwiping.value ? "none" : undefined,
}))

function onPointerDown(e: PointerEvent) {
    if (props.disabled || (e.pointerType === "mouse" && e.button !== 0)) return
    // never hijack drags that start on a control inside the row
    if ((e.target as HTMLElement).closest("[data-no-swipe]")) return
    startX = e.clientX
    startY = e.clientY
    pointerId = e.pointerId
    locked = undefined
}
function onPointerMove(e: PointerEvent) {
    if (pointerId !== e.pointerId) return
    const moveX = e.clientX - startX
    const moveY = e.clientY - startY
    if (!locked) {
        if (Math.abs(moveX) < 10 && Math.abs(moveY) < 10) return
        locked = Math.abs(moveX) > Math.abs(moveY) ? "x" : "y"
        if (locked === "x") {
            isSwiping.value = true
            try {
                ;(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId)
            } catch {
                /* pointer already gone - keep following the moves we get */
            }
        }
    }
    if (locked !== "x") return
    // resist beyond 1.6 x threshold
    const limit = props.threshold * 1.6
    dx.value = Math.max(-limit, Math.min(limit, moveX))
}
function onPointerUp(e: PointerEvent) {
    if (pointerId !== e.pointerId) return
    pointerId = undefined
    if (locked === "x") {
        suppressClick = true
        setTimeout(() => (suppressClick = false), 50)
        if (armedRight.value) emit("swipe-right")
        else if (armedLeft.value) emit("swipe-left")
    }
    isSwiping.value = false
    dx.value = 0
    locked = undefined
}
// a click that ends a swipe must not also trigger the row's tap action
function onClickCapture(e: MouseEvent) {
    if (suppressClick || isSwiping.value) {
        e.stopPropagation()
        e.preventDefault()
    }
}
</script>

<template>
    <div class="sm-swipe" :class="{ 'is-swiping': isSwiping }">
        <div class="sm-swipe__bg sm-swipe__bg--start" :class="{ 'is-armed': armedRight }" v-show="dx > 0">
            <slot name="start" :armed="armedRight" />
        </div>
        <div class="sm-swipe__bg sm-swipe__bg--end" :class="{ 'is-armed': armedLeft }" v-show="dx < 0">
            <slot name="end" :armed="armedLeft" />
        </div>
        <div
            class="sm-swipe__content"
            :style="style"
            @pointerdown="onPointerDown"
            @pointermove="onPointerMove"
            @pointerup="onPointerUp"
            @pointercancel="onPointerUp"
            @click.capture="onClickCapture"
        >
            <slot />
        </div>
    </div>
</template>

<style scoped>
.sm-swipe {
    position: relative;
    overflow: hidden;
    touch-action: pan-y;
}
.sm-swipe__content {
    position: relative;
    z-index: 1;
    background: var(--sm-surface, #fff);
    transition: transform 0.2s ease;
    will-change: transform;
}
.sm-swipe__bg {
    position: absolute;
    inset: 0;
    display: flex;
    align-items: center;
    padding: 0 1.25rem;
    color: #fff;
    font-weight: 600;
    transition: filter 0.15s ease;
}
.sm-swipe__bg--start {
    justify-content: flex-start;
    background: var(--sm-swipe-start, #16a34a);
}
.sm-swipe__bg--end {
    justify-content: flex-end;
    background: var(--sm-swipe-end, #dc2626);
}
.sm-swipe__bg:not(.is-armed) {
    filter: saturate(0.4) brightness(1.1);
}
</style>
