<!-- Day planner: a timeline per building (rooms × hours) with availability indicators. Click free space to book. -->
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"
import { LoadingContainer, Feedback, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useEntityStore as useBuildingStore, type Entity as Building } from "@/entities/buildings"
import { useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import DayTimeline from "@/components/planning/DayTimeline.vue"
import EquipmentToggles from "@/components/planning/EquipmentToggles.vue"
import { addDays, addMinutes, formatDay, fromDateKey, isSameDay, startOfDay, timeOnlyToMinutes, toDateKey, ReservationStatus } from "@/utilities/planning"

const route = useRoute()
const router = useRouter()
const feedback = useFeedback()

const { service: buildingService } = useBuildingStore()
const { service: roomService } = useRoomStore()
const { service: reservationService } = useReservationStore()

const buildings = ref<Array<Building>>([])
const buildingId = ref<number>()
const day = ref(startOfDay(fromDateKey(route.query.day) ?? new Date()))
const minCapacity = ref<number>()
const equipmentId = ref<Array<number>>()
const showInactive = ref(false)

const rooms = ref<Array<Room>>([])
const reservations = ref<Array<Reservation>>([])
const isLoading = ref(false)

const building = computed(() => buildings.value.find((b) => b.id === buildingId.value))
const startHour = computed(() => Math.floor(timeOnlyToMinutes(building.value?.opensAt, 7 * 60) / 60))
const endHour = computed(() => Math.ceil(timeOnlyToMinutes(building.value?.closesAt, 20 * 60) / 60))
const visibleRooms = computed(() => rooms.value.filter((r) => showInactive.value || r.isActive))
const stats = computed(() => {
    const live = reservations.value.filter((r) => !r.$isCancelled)
    return {
        bookings: live.length,
        pending: live.filter((r) => r.status === ReservationStatus.Pending).length,
        freeNow: isSameDay(day.value, new Date())
            ? visibleRooms.value.filter((room) => room.isActive && !live.some((r) => r.start! <= new Date() && r.end! > new Date() && r.rooms?.some((x) => x.roomId === room.id && x.approvalStatus !== "Rejected"))).length
            : undefined,
    }
})

async function loadBuildings() {
    buildings.value = await buildingService.list({ pageSize: 0 })
    const fromQuery = parseInt(String(route.query.buildingId ?? ""), 10)
    buildingId.value = buildings.value.find((b) => b.id === fromQuery)?.id ?? buildings.value[0]?.id
}
async function load() {
    if (!buildingId.value) return
    isLoading.value = true
    feedback.reset()
    try {
        const from = day.value
        const [roomResult, reservationResult] = await Promise.all([
            roomService.search({ buildingId: buildingId.value, minCapacity: minCapacity.value, equipmentId: equipmentId.value, includes: ["Equipment"], sortBy: ["Building"], pageSize: 0 }),
            reservationService.search({ buildingId: buildingId.value, from, to: addDays(from, 1), includes: ["Rooms"], pageSize: 0 }),
        ])
        rooms.value = roomResult.items
        reservations.value = reservationResult.items
    } catch (ex) {
        console.error(ex)
        feedback.fail("Loading failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}

function syncRoute() {
    router.replace({ query: { ...route.query, buildingId: buildingId.value, day: toDateKey(day.value) } })
}
watch([buildingId, day, minCapacity, () => equipmentId.value?.join(",")], () => {
    syncRoute()
    load()
})
onMounted(loadBuildings)

const shiftDay = (n: number) => (day.value = addDays(day.value, n))
const goToday = () => (day.value = startOfDay(new Date()))
const dayInput = computed({
    get: () => toDateKey(day.value),
    set: (v: string) => {
        const d = fromDateKey(v)
        if (d) day.value = d
    },
})

function book({ roomId, start }: { roomId: number; start: Date }) {
    router.push({ name: "ReservationDetails", params: { id: "new" }, query: { roomId, start: start.toISOString(), end: addMinutes(start, 60).toISOString() } })
}
const open = (r: Reservation) => router.push({ name: "ReservationDetails", params: { id: r.id } })
const openRoom = (r: Room) => router.push({ name: "RoomDetails", params: { id: r.id } })
</script>

<template>
    <section class="planner">
        <div class="d-flex flex-wrap align-items-center gap-2 mb-3">
            <h2 class="h4 mb-0 me-2"><i class="bi bi-calendar3-range me-2 text-primary"></i>{{ $t("planner") }}</h2>
            <div class="d-flex flex-wrap gap-1" role="group" :aria-label="$t('building')">
                <button v-for="b in buildings" :key="b.id" type="button" class="btn btn-sm" :class="b.id === buildingId ? 'btn-primary' : 'btn-outline-primary'" @click="buildingId = b.id">
                    <i class="bi bi-building me-1 d-none d-md-inline"></i>{{ b.title }}
                </button>
            </div>
        </div>

        <div class="card mb-3">
            <div class="card-body py-2">
                <div class="row g-2 align-items-center">
                    <div class="col-12 col-lg-auto d-flex flex-wrap align-items-center gap-1">
                        <button type="button" class="btn btn-outline-secondary btn-sm" :title="$t('previousDay')" @click="shiftDay(-1)"><i class="bi bi-chevron-left"></i></button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" @click="goToday">{{ $t("today") }}</button>
                        <button type="button" class="btn btn-outline-secondary btn-sm" :title="$t('nextDay')" @click="shiftDay(1)"><i class="bi bi-chevron-right"></i></button>
                        <input type="date" v-model="dayInput" class="form-control form-control-sm w-auto" />
                        <span class="fw-semibold ms-2 text-nowrap">{{ formatDay(day, { weekday: "long", day: "numeric", month: "long" }) }}</span>
                    </div>
                    <div class="col-6 col-lg-2">
                        <div class="input-group input-group-sm">
                            <span class="input-group-text"><i class="bi bi-people"></i></span>
                            <input type="number" min="1" v-model.lazy.number="minCapacity" class="form-control" :placeholder="$t('minCapacity')" />
                        </div>
                    </div>
                    <div class="col-6 col-lg-auto">
                        <div class="form-check form-switch mb-0">
                            <input id="planner-inactive" v-model="showInactive" type="checkbox" class="form-check-input" />
                            <label for="planner-inactive" class="form-check-label small">{{ $t("showOutOfService") }}</label>
                        </div>
                    </div>
                    <div class="col-12 col-lg">
                        <EquipmentToggles v-model="equipmentId" size="sm" />
                    </div>
                </div>
            </div>
        </div>

        <div class="d-flex flex-wrap gap-3 mb-2 small">
            <span><i class="bi bi-calendar-check text-primary me-1"></i>{{ stats.bookings }} {{ $t("bookings") }}</span>
            <span v-if="stats.pending"><i class="bi bi-hourglass-split text-warning me-1"></i>{{ stats.pending }} {{ $t("Pending") }}</span>
            <span v-if="stats.freeNow != null"><span class="legend-dot bg-success"></span>{{ stats.freeNow }} / {{ visibleRooms.length }} {{ $t("freeNow") }}</span>
            <span class="ms-auto text-muted d-none d-md-inline">
                <span class="legend-box dt-legend-approved"></span>{{ $t("Approved") }}
                <span class="legend-box dt-legend-pending ms-2"></span>{{ $t("Pending") }}
                <span class="legend-line ms-2"></span>{{ $t("now") }}
            </span>
        </div>

        <Feedback :feedback="feedback" />
        <LoadingContainer :is-loading="isLoading">
            <div class="card">
                <div class="card-body py-2">
                    <DayTimeline :rooms="visibleRooms" :reservations="reservations" :day="day" :start-hour="startHour" :end-hour="endHour" @select-slot="book" @open="open" @open-room="openRoom" />
                </div>
            </div>
            <p class="small text-muted mt-2"><i class="bi bi-info-circle me-1"></i>{{ $t("plannerHint") }}</p>
        </LoadingContainer>
    </section>
</template>

<style scoped>
.legend-dot {
    display: inline-block;
    width: 0.6rem;
    height: 0.6rem;
    border-radius: 50%;
    margin-right: 0.3rem;
}
.legend-box {
    display: inline-block;
    width: 0.9rem;
    height: 0.6rem;
    border-radius: 0.15rem;
    margin-right: 0.25rem;
    vertical-align: middle;
}
.dt-legend-approved {
    background: var(--bs-success-bg-subtle);
    border-left: 3px solid var(--bs-success);
}
.dt-legend-pending {
    background: repeating-linear-gradient(135deg, var(--bs-warning-bg-subtle), var(--bs-warning-bg-subtle) 3px, #fff 3px, #fff 6px);
    border-left: 3px solid var(--bs-warning);
}
.legend-line {
    display: inline-block;
    width: 2px;
    height: 0.8rem;
    background: var(--bs-danger);
    margin-right: 0.25rem;
    vertical-align: middle;
}
</style>
