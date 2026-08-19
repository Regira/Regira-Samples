<template>
    <div class="swipe-actions" :class="{ 'is-open': isOpen }">
        <div class="swipe-actions__behind" aria-hidden="true">
            <div class="swipe-actions__behind-left">
                <slot name="left" />
            </div>
            <div class="swipe-actions__behind-right">
                <slot name="right" />
            </div>
        </div>
        <div
            class="swipe-actions__surface"
            :style="surfaceStyle"
            @pointerdown="onPointerDown"
            @pointermove="onPointerMove"
            @pointerup="onPointerUp"
            @pointercancel="onPointerUp"
            @click="onSurfaceClick"
        >
            <slot />
        </div>
    </div>
</template>

<script setup lang="ts">
// A minimal, dependency-free swipe-to-reveal wrapper (pointer events cover touch + mouse + pen).
// Swipe left reveals the `right` slot (primary action, e.g. delete); swipe right reveals `left`
// (secondary action, e.g. toggle). Regira's UI kit ships no swipe primitive, so this is app-owned.
import { computed, ref } from "vue"

const props = withDefaults(
    defineProps<{
        maxReveal?: number // px each side can reveal
        disabled?: boolean
    }>(),
    { maxReveal: 84, disabled: false }
)
const emit = defineEmits<{ tap: []; "reveal-left": []; "reveal-right": [] }>()

const startX = ref(0)
const currentX = ref(0)
const dragging = ref(false)
const openOffset = ref(0) // -maxReveal..maxReveal, persists after release
const moved = ref(false)

const offset = computed(() => (dragging.value ? clamp(openOffset.value + (currentX.value - startX.value)) : openOffset.value))
const isOpen = computed(() => Math.abs(offset.value) > 4)
const surfaceStyle = computed(() => ({
    transform: `translateX(${offset.value}px)`,
    transition: dragging.value ? "none" : "transform 180ms ease",
}))

function clamp(v: number) {
    return Math.max(-props.maxReveal, Math.min(props.maxReveal, v))
}

function onPointerDown(e: PointerEvent) {
    if (props.disabled) return
    ;(e.target as HTMLElement).setPointerCapture?.(e.pointerId)
    startX.value = e.clientX
    currentX.value = e.clientX
    dragging.value = true
    moved.value = false
}
function onPointerMove(e: PointerEvent) {
    if (!dragging.value) return
    currentX.value = e.clientX
    if (Math.abs(currentX.value - startX.value) > 6) moved.value = true
}
function onPointerUp() {
    if (!dragging.value) return
    dragging.value = false
    const delta = currentX.value - startX.value
    const next = clamp(openOffset.value + delta)
    // snap: fully open past half the reveal width, otherwise closed
    if (next <= -props.maxReveal / 2) {
        openOffset.value = -props.maxReveal
        emit("reveal-right")
    } else if (next >= props.maxReveal / 2) {
        openOffset.value = props.maxReveal
        emit("reveal-left")
    } else {
        openOffset.value = 0
    }
}
function onSurfaceClick(e: MouseEvent) {
    if (moved.value) {
        // swallow the click that follows a drag so it doesn't also trigger a row navigation
        e.preventDefault()
        e.stopPropagation()
        moved.value = false
        return
    }
    if (isOpen.value) {
        // a tap while open just closes it
        e.preventDefault()
        e.stopPropagation()
        close()
    } else {
        emit("tap")
    }
}
function close() {
    openOffset.value = 0
}
defineExpose({ close })
</script>

<style scoped>
.swipe-actions {
    position: relative;
    overflow: hidden;
    touch-action: pan-y;
    border-radius: var(--sm-radius-lg, 14px);
}
.swipe-actions__behind {
    position: absolute;
    inset: 0;
    display: flex;
    justify-content: space-between;
    align-items: stretch;
}
.swipe-actions__behind-left,
.swipe-actions__behind-right {
    display: flex;
    align-items: stretch;
}
.swipe-actions__surface {
    position: relative;
    z-index: 1;
    background: var(--sm-surface, #fff);
    will-change: transform;
    cursor: pointer;
}
</style>
