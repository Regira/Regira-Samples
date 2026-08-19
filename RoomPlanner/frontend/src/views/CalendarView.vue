<!-- Calendar / timeline view of the day's reservations across rooms, with a live availability indicator
     per room. Not a generated slice view - a custom page built on top of the reservations + meeting-rooms
     stores, per the app's calendar-oriented interface requirement. -->
<template>
    <section>
        <div class="row align-items-end mb-3 gx-2">
            <div class="col-auto">
                <input type="date" v-model="dateInput" class="form-control" />
                <FormLabel :label="$t('date')" />
            </div>
            <div class="col-auto">
                <button type="button" class="btn btn-outline-secondary" @click="dateInput = todayInput">{{ $t("today") }}</button>
            </div>
            <div class="col-auto" style="min-width: 220px">
                <select v-model.number="buildingId" class="form-select">
                    <option :value="undefined">{{ $t("allBuildings") }}</option>
                    <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.title }}</option>
                </select>
                <FormLabel :label="$t('building')" />
            </div>
            <div class="col text-end small">
                <span class="badge text-bg-success me-2"><i class="bi bi-circle-fill"></i> {{ $t("availableNow") }}</span>
                <span class="badge text-bg-danger me-2"><i class="bi bi-circle-fill"></i> {{ $t("occupiedNow") }}</span>
                <span class="badge text-bg-warning">Pending</span>
                <span class="badge text-bg-success">Approved</span>
            </div>
        </div>

        <LoadingContainer :is-loading="isLoading">
            <div v-if="rooms.length === 0" class="italic-muted">{{ $t("noResults") }}</div>
            <div v-else class="timeline">
                <div class="row border-bottom pb-1 mb-2 fw-bold text-muted small">
                    <div class="col-3 col-md-2">{{ $t("room") }}</div>
                    <div class="col-9 col-md-10">
                        <div class="d-flex">
                            <div v-for="h in hours" :key="h" class="flex-fill text-center" style="font-size: 0.75rem">{{ h }}:00</div>
                        </div>
                    </div>
                </div>

                <div v-for="room in rooms" :key="room.id" class="row align-items-center py-2 border-bottom">
                    <div class="col-3 col-md-2 text-truncate">
                        <span class="badge rounded-pill me-1" :class="isOccupiedNow(room) ? 'text-bg-danger' : 'text-bg-success'">
                            <i class="bi bi-circle-fill"></i>
                        </span>
                        <RouterLink :to="{ name: 'MeetingRoomDetails', params: { id: room.id } }">{{ room.title }}</RouterLink>
                        <div class="text-muted small">{{ room.floor?.title }} &middot; {{ $t("capacity") }} {{ room.capacity }}</div>
                    </div>
                    <div class="col-9 col-md-10">
                        <div class="position-relative timeline-track">
                            <div
                                v-for="block in blocksFor(room)"
                                :key="block.id"
                                class="timeline-block"
                                :class="block.status === 'Approved' ? 'bg-success' : 'bg-warning'"
                                :style="{ left: block.left + '%', width: block.width + '%' }"
                                :title="`${block.title} (${block.startLabel} - ${block.endLabel}) - ${block.status}`"
                            >
                                <RouterLink :to="{ name: 'ReservationDetails', params: { id: block.id } }" class="stretched-link text-truncate d-block">
                                    {{ block.title }}
                                </RouterLink>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </LoadingContainer>
    </section>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from "vue"
import { RouterLink } from "vue-router"
import { LoadingContainer, FormLabel } from "@regira/modules/vue/ui"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import { useEntityStore as useMeetingRoomStore, type Entity as MeetingRoom } from "@/entities/meeting-rooms"
import { useEntityStore as useBuildingStore, type Entity as Building } from "@/entities/buildings"

const DAY_START_HOUR = 7
const DAY_END_HOUR = 19
const hours = Array.from({ length: DAY_END_HOUR - DAY_START_HOUR }, (_, i) => DAY_START_HOUR + i)

function toInputDate(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`
}
const todayInput = toInputDate(new Date())
const dateInput = ref(todayInput)
const buildingId = ref<number | undefined>(undefined)

const { service: reservationService } = useReservationStore()
const { service: roomService } = useMeetingRoomStore()
const { service: buildingService } = useBuildingStore()

const isLoading = ref(false)
const reservations = ref<Array<Reservation>>([])
const rooms = ref<Array<MeetingRoom>>([])
const buildings = ref<Array<Building>>([])

async function load() {
    isLoading.value = true
    try {
        const [y, m, d] = dateInput.value.split("-").map(Number)
        const dayStart = new Date(y!, m! - 1, d!, 0, 0, 0)
        const dayEnd = new Date(y!, m! - 1, d!, 23, 59, 59)

        const [resResult, roomResult] = await Promise.all([
            reservationService.search({ minStartTime: dayStart, maxStartTime: dayEnd, pageSize: 0 }),
            roomService.search({ buildingId: buildingId.value, isActive: true, pageSize: 0, sortBy: ["Title"] }),
        ])
        reservations.value = resResult.items
        rooms.value = roomResult.items
    } finally {
        isLoading.value = false
    }
}

onMounted(async () => {
    buildings.value = await buildingService.list({ pageSize: 0 })
    await load()
})
watch([dateInput, buildingId], load)

function timeLabel(d?: Date): string {
    return d ? new Date(d).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : ""
}

function blocksFor(room: MeetingRoom) {
    const totalHours = DAY_END_HOUR - DAY_START_HOUR
    return reservations.value
        .filter((r) => r.status !== "Cancelled" && r.status !== "Rejected")
        .filter((r) => (r.rooms ?? []).some((rr) => rr.roomId === room.id))
        .map((r) => {
            const start = new Date(r.startTime)
            const end = new Date(r.endTime)
            const startFraction = Math.max(0, (start.getHours() + start.getMinutes() / 60 - DAY_START_HOUR) / totalHours)
            const endFraction = Math.min(1, (end.getHours() + end.getMinutes() / 60 - DAY_START_HOUR) / totalHours)
            return {
                id: r.id,
                title: r.title,
                status: r.status,
                left: startFraction * 100,
                width: Math.max(1, (endFraction - startFraction) * 100),
                startLabel: timeLabel(start),
                endLabel: timeLabel(end),
            }
        })
        .filter((b) => b.width > 0)
}

function isOccupiedNow(room: MeetingRoom): boolean {
    if (dateInput.value !== todayInput) return false
    const now = new Date()
    return reservations.value
        .filter((r) => r.status === "Approved")
        .filter((r) => (r.rooms ?? []).some((rr) => rr.roomId === room.id))
        .some((r) => new Date(r.startTime) <= now && now <= new Date(r.endTime))
}
</script>

<style scoped>
.timeline-track {
    height: 2.25rem;
    background: repeating-linear-gradient(to right, var(--bs-border-color) 0, var(--bs-border-color) 1px, transparent 1px, transparent calc(100% / 12));
    border: 1px solid var(--bs-border-color);
    border-radius: 0.25rem;
}
.timeline-block {
    position: absolute;
    top: 2px;
    bottom: 2px;
    border-radius: 0.2rem;
    color: #fff;
    font-size: 0.75rem;
    padding: 0.1rem 0.35rem;
    overflow: hidden;
    white-space: nowrap;
}
.timeline-block .stretched-link {
    color: inherit;
}
</style>
