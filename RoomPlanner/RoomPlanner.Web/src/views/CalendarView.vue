<!-- Week calendar: bookings of one room, one employee (organizer or attendee) or one building, laid out per day and hour. -->
<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import { LoadingContainer, Feedback, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { InputSelector as RoomInputSelector, useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import { InputSelector as EmployeeInputSelector, type Entity as Employee } from "@/entities/employees"
import { InputSelector as BuildingInputSelector, type Entity as Building } from "@/entities/buildings"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import StatusBadge from "@/components/planning/StatusBadge.vue"
import { addDays, addMinutes, formatDay, formatTime, fromDateKey, isSameDay, startOfWeek, statusVariant, toDateKey, MINUTE, ReservationStatus } from "@/utilities/planning"

const route = useRoute()
const router = useRouter()
const feedback = useFeedback()
const { service } = useReservationStore()
const { fromPool: poolRoom } = useRoomStore()

const START_HOUR = 7
const END_HOUR = 20
const HOUR_PX = 48

const intId = (v: unknown) => {
    const n = parseInt(String(v ?? ""), 10)
    return isNaN(n) ? undefined : n
}
const weekStart = ref(startOfWeek(fromDateKey(route.query.week) ?? new Date()))
const roomId = ref<number | undefined>(intId(route.query.roomId))
const employeeId = ref<number | undefined>(intId(route.query.employeeId))
const buildingId = ref<number | undefined>(intId(route.query.buildingId))
const room = ref<Room>()
const employee = ref<Employee>()
const building = ref<Building>()
const showWeekend = ref(false)
const hideCancelled = ref(true)

const days = computed(() => Array.from({ length: showWeekend.value ? 7 : 5 }, (_, i) => addDays(weekStart.value, i)))
const hours = Array.from({ length: END_HOUR - START_HOUR }, (_, i) => START_HOUR + i)
const items = ref<Array<Reservation>>([])
const isLoading = ref(false)
const hasScope = computed(() => roomId.value != null || employeeId.value != null || buildingId.value != null)

async function load() {
    router.replace({ query: { week: toDateKey(weekStart.value), roomId: roomId.value, employeeId: employeeId.value, buildingId: buildingId.value } })
    if (!hasScope.value) {
        items.value = []
        return
    }
    isLoading.value = true
    feedback.reset()
    try {
        const { items: result } = await service.search({
            roomId: roomId.value,
            employeeId: employeeId.value,
            buildingId: buildingId.value,
            from: weekStart.value,
            to: addDays(weekStart.value, 7),
            status: hideCancelled.value ? [ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.PartiallyApproved, ReservationStatus.Rejected] : undefined,
            includes: ["Rooms"],
            pageSize: 0,
        })
        items.value = result
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}
watch([weekStart, roomId, employeeId, buildingId, hideCancelled], load, { immediate: true })

// lay out the bookings of one day in side-by-side lanes when they overlap
type Block = { r: Reservation; top: number; height: number; lane: number; lanes: number }
function blocksFor(day: Date): Array<Block> {
    const dayStart = addMinutes(new Date(day), START_HOUR * 60)
    const list = items.value.filter((r) => r.start && r.end && isSameDay(r.start, day)).sort((a, b) => a.start!.getTime() - b.start!.getTime())
    const blocks: Array<Block> = []
    let group: Array<Block> = []
    let groupEnd = 0
    const flush = () => {
        const lanes = Math.max(1, ...group.map((b) => b.lane + 1))
        group.forEach((b) => (b.lanes = lanes))
        group = []
    }
    for (const r of list) {
        const top = Math.max(0, ((r.start!.getTime() - dayStart.getTime()) / MINUTE / 60) * HOUR_PX)
        const height = Math.max(18, ((r.end!.getTime() - r.start!.getTime()) / MINUTE / 60) * HOUR_PX - 2)
        if (group.length && r.start!.getTime() >= groupEnd) flush()
        const used = new Set(group.filter((b) => b.r.end!.getTime() > r.start!.getTime()).map((b) => b.lane))
        let lane = 0
        while (used.has(lane)) lane++
        const block = { r, top, height, lane, lanes: 1 }
        group.push(block)
        blocks.push(block)
        groupEnd = Math.max(groupEnd, r.end!.getTime())
    }
    flush()
    return blocks
}

function clickSlot(day: Date, ev: MouseEvent) {
    const rect = (ev.currentTarget as HTMLElement).getBoundingClientRect()
    const minutes = Math.floor(((ev.clientY - rect.top) / HOUR_PX) * 2) * 30
    const start = addMinutes(new Date(day), START_HOUR * 60 + minutes)
    router.push({
        name: "ReservationDetails",
        params: { id: "new" },
        query: { start: start.toISOString(), end: addMinutes(start, 60).toISOString(), roomId: roomId.value, organizerId: employeeId.value },
    })
}
const roomOf = (r: Reservation) => (r.rooms?.[0]?.room ? poolRoom(r.rooms[0].room as Room) : undefined)
const isToday = (d: Date) => isSameDay(d, new Date())
const shiftWeek = (n: number) => (weekStart.value = addDays(weekStart.value, 7 * n))
function clearScope() {
    roomId.value = employeeId.value = buildingId.value = undefined
    room.value = undefined
    employee.value = undefined
    building.value = undefined
}
</script>

<template>
    <section class="calendar">
        <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
            <h2 class="h4 mb-0 me-2"><i class="bi bi-calendar-week me-2 text-primary"></i>{{ $t("calendar") }}</h2>
            <button type="button" class="btn btn-outline-secondary btn-sm" @click="shiftWeek(-1)"><i class="bi bi-chevron-left"></i></button>
            <button type="button" class="btn btn-outline-secondary btn-sm" @click="weekStart = startOfWeek(new Date())">{{ $t("thisWeek") }}</button>
            <button type="button" class="btn btn-outline-secondary btn-sm" @click="shiftWeek(1)"><i class="bi bi-chevron-right"></i></button>
            <span class="fw-semibold">{{ formatDay(days[0]) }} – {{ formatDay(days[days.length - 1]) }}</span>
        </div>

        <div class="card mb-3">
            <div class="card-body py-2">
                <div class="row g-2 align-items-center">
                    <div class="col-md-4">
                        <RoomInputSelector v-model="room" v-model:idValue="roomId" :canEdit="false" :placeholder="$t('room')" />
                    </div>
                    <div class="col-md-4">
                        <EmployeeInputSelector v-model="employee" v-model:idValue="employeeId" :canEdit="false" :placeholder="$t('participant')" />
                    </div>
                    <div class="col-md-4">
                        <BuildingInputSelector v-model="building" v-model:idValue="buildingId" :canEdit="false" :placeholder="$t('building')" />
                    </div>
                    <div class="col-12 d-flex flex-wrap gap-3 small">
                        <div class="form-check form-switch mb-0">
                            <input id="cal-weekend" v-model="showWeekend" type="checkbox" class="form-check-input" />
                            <label for="cal-weekend" class="form-check-label">{{ $t("showWeekend") }}</label>
                        </div>
                        <div class="form-check form-switch mb-0">
                            <input id="cal-cancelled" v-model="hideCancelled" type="checkbox" class="form-check-input" />
                            <label for="cal-cancelled" class="form-check-label">{{ $t("hideCancelled") }}</label>
                        </div>
                        <a v-if="hasScope" href="#" class="ms-auto" @click.prevent="clearScope"><i class="bi bi-x-circle me-1"></i>{{ $t("clear") }}</a>
                    </div>
                </div>
            </div>
        </div>

        <Feedback :feedback="feedback" />
        <p v-if="!hasScope" class="alert alert-info"><i class="bi bi-info-circle me-1"></i>{{ $t("calendarHint") }}</p>
        <LoadingContainer v-else :is-loading="isLoading">
            <div class="week-scroll">
                <div class="week-grid" :style="{ '--hour-px': `${HOUR_PX}px`, '--days': days.length }">
                    <div class="wg-corner"></div>
                    <div v-for="d in days" :key="d.getTime()" class="wg-dayhead" :class="{ 'is-today': isToday(d) }">
                        {{ formatDay(d) }}
                    </div>
                    <div class="wg-hours">
                        <div v-for="h in hours" :key="h" class="wg-hour">{{ h }}:00</div>
                    </div>
                    <div v-for="d in days" :key="`c${d.getTime()}`" class="wg-day" :class="{ 'is-today': isToday(d) }" @click="clickSlot(d, $event)">
                        <div v-for="h in hours" :key="h" class="wg-slot"></div>
                        <div
                            v-for="b in blocksFor(d)"
                            :key="b.r.id"
                            class="wg-block"
                            :class="[`wg-${statusVariant[b.r.status]}`, { 'is-cancelled': b.r.$isCancelled }]"
                            :style="{
                                top: `${b.top}px`,
                                height: `${b.height}px`,
                                left: `calc(${(b.lane / b.lanes) * 100}% + 2px)`,
                                width: `calc(${100 / b.lanes}% - 4px)`,
                                borderLeftColor: roomOf(b.r)?.$color,
                            }"
                            :title="`${b.r.title} · ${b.r.organizer?.title ?? ''}`"
                            @click.stop="router.push({ name: 'ReservationDetails', params: { id: b.r.id } })"
                        >
                            <div class="fw-semibold text-truncate">{{ b.r.title }}</div>
                            <div class="text-truncate">{{ formatTime(b.r.start) }}–{{ formatTime(b.r.end) }} · {{ b.r.rooms?.map((x) => x.room?.title).join(", ") }}</div>
                            <StatusBadge v-if="b.height > 50" :status="b.r.status" compact />
                        </div>
                    </div>
                </div>
            </div>
            <p class="small text-muted mt-2"><i class="bi bi-info-circle me-1"></i>{{ items.length }} {{ $t("bookings") }} · {{ $t("calendarClickHint") }}</p>
        </LoadingContainer>
    </section>
</template>

<style scoped>
.week-scroll {
    overflow-x: auto;
}
.week-grid {
    display: grid;
    grid-template-columns: 3.5rem repeat(var(--days), minmax(7.5rem, 1fr));
    min-width: calc(3.5rem + var(--days) * 7.5rem);
    font-size: 0.8rem;
}
.wg-dayhead {
    text-align: center;
    font-weight: 600;
    padding: 0.25rem;
    border-bottom: 2px solid var(--bs-border-color);
}
.wg-dayhead.is-today {
    color: var(--bs-primary);
}
.wg-hours {
    grid-row: 2;
}
.wg-hour {
    height: var(--hour-px);
    color: var(--bs-secondary-color);
    text-align: right;
    padding-right: 0.4rem;
    transform: translateY(-0.5em);
}
.wg-day {
    grid-row: 2;
    position: relative;
    border-left: 1px solid var(--bs-border-color);
    cursor: copy;
}
.wg-day.is-today {
    background: rgba(var(--bs-primary-rgb), 0.04);
}
.wg-slot {
    height: var(--hour-px);
    border-bottom: 1px dashed var(--bs-border-color-translucent);
}
.wg-block {
    position: absolute;
    border-radius: 0.3rem;
    border-left: 4px solid var(--bs-secondary);
    padding: 0.1rem 0.3rem;
    overflow: hidden;
    cursor: pointer;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.15);
    line-height: 1.25;
}
.wg-block:hover {
    z-index: 5;
    filter: brightness(0.93);
}
.wg-success {
    background: var(--bs-success-bg-subtle);
}
.wg-info {
    background: var(--bs-info-bg-subtle);
}
.wg-warning {
    background: repeating-linear-gradient(135deg, var(--bs-warning-bg-subtle), var(--bs-warning-bg-subtle) 6px, #fff3cd99 6px, #fff3cd99 12px);
}
.wg-danger {
    background: var(--bs-danger-bg-subtle);
}
.wg-secondary,
.is-cancelled {
    background: var(--bs-tertiary-bg);
    text-decoration: line-through;
    opacity: 0.75;
}
</style>
