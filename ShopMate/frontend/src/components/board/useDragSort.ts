import { ref, type Ref } from "vue"

/**
 * Pointer-based drag sorting for a vertical list (touch + mouse). Bind `start/move/end` on a drag handle
 * that carries `touch-action: none`; each row element carries `data-drag-row` (its height is the step size).
 * The array is reordered live while dragging; `onDrop(moved)` fires once on release.
 */
export function useDragSort<T>(items: Ref<Array<T>>, onDrop: (moved: boolean) => void) {
    const dragIndex = ref<number>()
    const offsetY = ref(0)
    let startY = 0
    let rowHeight = 56
    let pointerId: number | undefined
    let origin = -1

    function start(e: PointerEvent, index: number) {
        if (e.pointerType === "mouse" && e.button !== 0) return
        const row = (e.currentTarget as HTMLElement).closest("[data-drag-row]") as HTMLElement | null
        rowHeight = row?.offsetHeight || rowHeight
        startY = e.clientY
        origin = index
        dragIndex.value = index
        offsetY.value = 0
        pointerId = e.pointerId
        try {
            ;(e.currentTarget as HTMLElement).setPointerCapture(e.pointerId)
        } catch {
            /* pointer already released (e.g. a synthetic event) - moves still arrive on the handle */
        }
        e.preventDefault()
    }

    function move(e: PointerEvent) {
        if (pointerId !== e.pointerId || dragIndex.value == null) return
        const steps = Math.round((e.clientY - startY) / rowHeight)
        if (steps !== 0) {
            const from = dragIndex.value
            const to = Math.max(0, Math.min(items.value.length - 1, from + steps))
            if (to !== from) {
                const next = [...items.value]
                const [moved] = next.splice(from, 1)
                next.splice(to, 0, moved!)
                items.value = next
                startY += (to - from) * rowHeight
                dragIndex.value = to
            }
        }
        offsetY.value = e.clientY - startY
    }

    function end(e: PointerEvent) {
        if (pointerId !== e.pointerId) return
        pointerId = undefined
        const moved = dragIndex.value !== origin
        dragIndex.value = undefined
        offsetY.value = 0
        onDrop(moved)
    }

    function moveBy(index: number, delta: number) {
        const to = index + delta
        if (to < 0 || to >= items.value.length) return
        const next = [...items.value]
        const [moved] = next.splice(index, 1)
        next.splice(to, 0, moved!)
        items.value = next
        onDrop(true)
    }

    return { dragIndex, offsetY, start, move, end, moveBy }
}

export default useDragSort
