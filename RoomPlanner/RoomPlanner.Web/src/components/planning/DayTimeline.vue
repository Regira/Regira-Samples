<!-- Presentational day timeline: one row per room, one column per hour, reservations as bars.
     Emits `select-slot` (click on free space, snapped to 30 minutes) and `open` (click on a bar). -->
<script setup lang="ts">
import { computed, onBeforeUnmount, ref } from "vue"
import type { Entity as Room } from "@/entities/rooms"
import type { Entity as Reservation } from "@/entities/reservations"
import { addMinutes, formatTime, isSameDay, startOfDay, statusVariant, MINUTE, RoomApprovalStatus } from "@/utilities/planning"

const props = withDefaults(
    defineProps<{
        rooms: Array<Room>
        reservations: Array<Reservation>
        day: Date
        startHour?: number
        endHour?: number
        highlight?: { start: Date; end: Date } // e.g. the slot being edited
        excludeId?: number // hide this reservation (the one being edited)
        compact?: boolean
        showRoomInfo?: boolean
        showHeader?: boolean
    }>(),
    { startHour: 7, endHour: 20, showRoomInfo: true, showHeader: true }
)
const emit = defineEmits<{ "select-slot": [{ roomId: number; start: Date }]; open: [Reservation]; "open-room": [Room] }>()

const hours = computed(() => Array.from({ length: props.endHour - props.startHour }, (_, i) => props.startHour + i))
const dayStart = computed(() => startOfDay(props.day))
const windowStart = computed(() => addMinutes(dayStart.value, props.startHour * 60))
const windowMinutes = computed(() => (props.endHour - props.startHour) * 60)

const pct = (d: Date) => Math.min(100, Math.max(0, ((d.getTime() - windowStart.value.getTime()) / MINUTE / windowMinutes.value) * 100))

type Bar = { reservation: Reservation; left: number; width: number; pending: boolean; rejected: boolean }
const barsByRoom = computed(() => {
    const map = new Map<number, Array<Bar>>()
    const windowEnd = addMinutes(windowStart.value, windowMinutes.value)
    for (const r of props.reservations) {
        if (!r.start || !r.end || r.id === props.excludeId || r.$isCancelled) continue
        if (!(r.start < windowEnd && r.end > windowStart.value)) continue
        for (const row of r.rooms ?? []) {
            if (row.approvalStatus === RoomApprovalStatus.Rejected) continue
            const left = pct(r.start)
            const bars = map.get(row.roomId) ?? []
            bars.push({ reservation: r, left, width: Math.max(0.8, pct(r.end) - left), pending: row.approvalStatus === RoomApprovalStatus.Pending, rejected: false })
            map.set(row.roomId, bars)
        }
    }
    return map
})

// availability indicator: booked minutes of the visible window + "busy now" when the day is today
const now = ref(new Date())
const timer = setInterval(() => (now.value = new Date()), 60_000)
onBeforeUnmount(() => clearInterval(timer))
const isToday = computed(() => isSameDay(props.day, now.value))
const nowPct = computed(() => (isToday.value ? pct(now.value) : undefined))
function occupancy(roomId: number) {
    const bars = barsByRoom.value.get(roomId) ?? []
    const booked = bars.reduce((sum, b) => sum + b.width, 0)
    return Math.min(100, Math.round(booked))
}
function busyNow(roomId: number) {
    if (!isToday.value) return undefined
    return (barsByRoom.value.get(roomId) ?? []).find((b) => b.reservation.start! <= now.value && b.reservation.end! > now.value)
}
function indicator(room: Room) {
    if (!room.isActive) return { cls: "bg-secondary", title: "outOfService" }
    const occ = occupancy(room.id)
    if (isToday.value) return busyNow(room.id) ? { cls: "bg-danger", title: "busyNow" } : { cls: "bg-success", title: "freeNow" }
    return occ >= 70 ? { cls: "bg-danger", title: "mostlyBooked" } : occ >= 35 ? { cls: "bg-warning", title: "partlyBooked" } : { cls: "bg-success", title: "mostlyFree" }
}

function handleTrackClick(room: Room, ev: MouseEvent) {
    if (!room.isActive) return
    const el = ev.currentTarget as HTMLElement
    const rect = el.getBoundingClientRect()
    const ratio = (ev.clientX - rect.left) / rect.width
    const minutes = Math.floor((ratio * windowMinutes.value) / 30) * 30
    emit("select-slot", { roomId: room.id, start: addMinutes(windowStart.value, minutes) })
}
</script>

<template>
    <div class="day-timeline" :class="{ 'is-compact': compact }">
        <div v-if="showHeader" class="dt-row dt-head">
            <div v-if="showRoomInfo" class="dt-room"></div>
            <div class="dt-track">
                <span v-for="h in hours" :key="h" class="dt-hour" :style="{ left: `${((h - startHour) / (endHour - startHour)) * 100}%` }">{{ h }}:00</span>
            </div>
        </div>
        <div v-for="room in rooms" :key="room.id" class="dt-row" :class="{ 'is-inactive': !room.isActive }">
            <div v-if="showRoomInfo" class="dt-room text-truncate">
                <span class="availability-dot" :class="indicator(room).cls" :title="$t(indicator(room).title)"></span>
                <a href="#" class="fw-semibold text-decoration-none" @click.prevent="emit('open-room', room)">{{ room.title }}</a>
                <small class="text-muted ms-1 d-none d-md-inline"><i class="bi bi-people"></i> {{ room.capacity }}</small>
                <i v-if="room.requiresApproval" class="bi bi-shield-lock text-warning ms-1 small" :title="$t('requiresApproval')"></i>
                <div v-if="!compact" class="small text-muted text-truncate d-none d-md-block">
                    <i v-for="eq in room.equipment" :key="eq.id" :class="`bi bi-${eq.equipment?.icon || 'tools'}`" class="me-1" :title="eq.equipment?.title"></i>
                    <span class="ms-1">{{ occupancy(room.id) }}% {{ $t("booked") }}</span>
                </div>
            </div>
            <div class="dt-track" :title="room.isActive ? $t('clickToBook') : $t('outOfService')" @click="handleTrackClick(room, $event)">
                <span v-for="h in hours" :key="h" class="dt-grid" :style="{ left: `${((h - startHour) / (endHour - startHour)) * 100}%` }"></span>
                <div
                    v-if="highlight"
                    class="dt-highlight"
                    :style="{ left: `${pct(highlight.start)}%`, width: `${Math.max(0.8, pct(highlight.end) - pct(highlight.start))}%` }"
                ></div>
                <div
                    v-for="bar in barsByRoom.get(room.id) ?? []"
                    :key="bar.reservation.id"
                    class="dt-bar"
                    :class="[`dt-${statusVariant[bar.reservation.status]}`, { 'is-pending': bar.pending }]"
                    :style="{ left: `${bar.left}%`, width: `${bar.width}%`, borderLeftColor: room.$color }"
                    :title="`${formatTime(bar.reservation.start)}–${formatTime(bar.reservation.end)} ${bar.reservation.title} (${bar.reservation.organizer?.title ?? ''}) · ${$t(bar.reservation.status)}`"
                    @click.stop="emit('open', bar.reservation)"
                >
                    <span class="dt-bar-text">{{ formatTime(bar.reservation.start) }} {{ bar.reservation.title }}</span>
                </div>
                <div v-if="nowPct != null && nowPct > 0 && nowPct < 100" class="dt-now" :style="{ left: `${nowPct}%` }"></div>
            </div>
        </div>
        <p v-if="!rooms.length" class="italic-muted my-2">{{ $t("noRooms") }}</p>
    </div>
</template>

<style scoped lang="scss">
.day-timeline {
    --dt-room-width: 15rem;
    --dt-row-height: 3rem;
    font-size: 0.875rem;
    user-select: none;
}
.is-compact {
    --dt-room-width: 9rem;
    --dt-row-height: 2.25rem;
}
.dt-row {
    display: flex;
    align-items: stretch;
    border-bottom: 1px solid var(--bs-border-color);
    min-height: var(--dt-row-height);
}
.dt-row.is-inactive .dt-track {
    background: repeating-linear-gradient(45deg, var(--bs-tertiary-bg), var(--bs-tertiary-bg) 6px, transparent 6px, transparent 12px);
    cursor: not-allowed;
}
.dt-head {
    min-height: 1.5rem;
    border-bottom-width: 2px;
}
.dt-room {
    flex: 0 0 var(--dt-room-width);
    max-width: var(--dt-room-width);
    padding: 0.25rem 0.5rem 0.25rem 0;
}
.dt-track {
    position: relative;
    flex: 1 1 auto;
    min-width: 0;
    cursor: copy;
}
.dt-head .dt-track {
    cursor: default;
}
.dt-hour {
    position: absolute;
    top: 0.1rem;
    transform: translateX(-50%);
    font-size: 0.7rem;
    color: var(--bs-secondary-color);
    white-space: nowrap;
}
.dt-hour:first-child {
    transform: none;
}
.dt-grid {
    position: absolute;
    top: 0;
    bottom: 0;
    border-left: 1px dashed var(--bs-border-color-translucent);
}
.dt-bar {
    position: absolute;
    top: 15%;
    bottom: 15%;
    border-radius: 0.3rem;
    border-left: 4px solid;
    overflow: hidden;
    cursor: pointer;
    padding: 0 0.25rem;
    display: flex;
    align-items: center;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.15);
    transition: filter 0.1s;
}
.dt-bar:hover {
    filter: brightness(0.92);
    z-index: 2;
}
.dt-bar-text {
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    font-size: 0.75rem;
}
.dt-success {
    background: var(--bs-success-bg-subtle);
    color: var(--bs-success-text-emphasis);
}
.dt-info {
    background: var(--bs-info-bg-subtle);
    color: var(--bs-info-text-emphasis);
}
.dt-warning,
.is-pending {
    background: repeating-linear-gradient(135deg, var(--bs-warning-bg-subtle), var(--bs-warning-bg-subtle) 6px, #fff3cd99 6px, #fff3cd99 12px);
    color: var(--bs-warning-text-emphasis);
}
.dt-danger {
    background: var(--bs-danger-bg-subtle);
    color: var(--bs-danger-text-emphasis);
}
.dt-highlight {
    position: absolute;
    top: 5%;
    bottom: 5%;
    border: 2px dashed var(--bs-primary);
    border-radius: 0.3rem;
    background: rgba(var(--bs-primary-rgb), 0.08);
    z-index: 1;
    pointer-events: none;
}
.dt-now {
    position: absolute;
    top: 0;
    bottom: 0;
    border-left: 2px solid var(--bs-danger);
    z-index: 3;
    pointer-events: none;
}
.availability-dot {
    display: inline-block;
    width: 0.6rem;
    height: 0.6rem;
    border-radius: 50%;
    margin-right: 0.35rem;
}
@media (max-width: 575.98px) {
    .day-timeline {
        --dt-room-width: 7rem;
    }
    .dt-hour:nth-child(even) {
        display: none;
    }
}
</style>
