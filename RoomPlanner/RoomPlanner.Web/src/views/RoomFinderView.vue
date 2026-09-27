<!-- "Find a room": pick a slot, a head count and equipment → room cards with an availability indicator and the day's bookings. -->
<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue"
import { useRouter } from "vue-router"
import { LoadingContainer, Feedback, FormLabel, useFeedback, toFeedbackError } from "@regira/modules/vue/ui"
import { useEntityStore as useBuildingStore, type Entity as Building } from "@/entities/buildings"
import { useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import EquipmentToggles from "@/components/planning/EquipmentToggles.vue"
import DayTimeline from "@/components/planning/DayTimeline.vue"
import { addDays, addMinutes, formatDuration, formatRange, fromLocalInput, nextQuarter, startOfDay, toLocalInput, ReservationStatus } from "@/utilities/planning"

const router = useRouter()
const feedback = useFeedback()
const { service: buildingService } = useBuildingStore()
const { service: roomService } = useRoomStore()
const { service: reservationService } = useReservationStore()

const buildings = ref<Array<Building>>([])
// default slot: the next quarter hour, or 09:00 on the next working day outside office hours
function defaultStart(): Date {
    const d = nextQuarter()
    const weekend = (x: Date) => x.getDay() === 0 || x.getDay() === 6
    if (!weekend(d) && d.getHours() >= 8 && d.getHours() < 17) return d
    let next = startOfDay(d.getHours() < 8 && !weekend(d) ? d : addDays(d, 1))
    while (weekend(next)) next = addDays(next, 1)
    next.setHours(9)
    return next
}
const start = ref(toLocalInput(defaultStart()))
const duration = ref(60)
const people = ref(4)
const buildingId = ref<number>()
const equipmentId = ref<Array<number>>()
const onlyAvailable = ref(false)

const slotStart = computed(() => fromLocalInput(start.value) ?? nextQuarter())
const slotEnd = computed(() => addMinutes(slotStart.value, duration.value))

const matching = ref<Array<Room>>([])
const availableIds = ref(new Set<number>())
const dayBookings = ref<Array<Reservation>>([])
const isLoading = ref(false)

const cards = computed(() =>
    matching.value
        .map((room) => ({ room, available: availableIds.value.has(room.id) }))
        .filter((x) => !onlyAvailable.value || x.available)
        .sort((a, b) => Number(b.available) - Number(a.available) || a.room.capacity - b.room.capacity)
)
const availableCount = computed(() => matching.value.filter((r) => availableIds.value.has(r.id)).length)

async function search() {
    isLoading.value = true
    feedback.reset()
    try {
        const base = { buildingId: buildingId.value, minCapacity: people.value || undefined, equipmentId: equipmentId.value, isActive: true, includes: ["Equipment"], pageSize: 0 }
        const dayStart = startOfDay(slotStart.value)
        const [all, free, bookings] = await Promise.all([
            roomService.search({ ...base, sortBy: ["Capacity"] }),
            roomService.search({ ...base, availableFrom: slotStart.value, availableTo: slotEnd.value }),
            reservationService.search({
                buildingId: buildingId.value,
                from: dayStart,
                to: addDays(dayStart, 1),
                status: [ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.PartiallyApproved],
                includes: ["Rooms"],
                pageSize: 0,
            }),
        ])
        matching.value = all.items
        availableIds.value = new Set(free.items.map((r) => r.id))
        dayBookings.value = bookings.items
    } catch (ex) {
        console.error(ex)
        feedback.fail("Search failed", toFeedbackError(ex))
    } finally {
        isLoading.value = false
    }
}
onMounted(async () => {
    buildings.value = await buildingService.list({ pageSize: 0 })
    await search()
})
watch([start, duration, people, buildingId, () => equipmentId.value?.join(",")], search)

function book(room: Room) {
    router.push({ name: "ReservationDetails", params: { id: "new" }, query: { roomId: room.id, start: slotStart.value.toISOString(), end: slotEnd.value.toISOString() } })
}
</script>

<template>
    <section class="room-finder">
        <h2 class="h4 mb-3"><i class="bi bi-search me-2 text-primary"></i>{{ $t("findARoom") }}</h2>

        <div class="card mb-3">
            <div class="card-body">
                <div class="row g-2">
                    <div class="col-md-4">
                        <input type="datetime-local" v-model="start" step="900" class="form-control" />
                        <FormLabel :label="$t('start')" />
                    </div>
                    <div class="col-6 col-md-2">
                        <select v-model.number="duration" class="form-select">
                            <option v-for="d in [30, 60, 90, 120, 180, 240, 480]" :key="d" :value="d">{{ formatDuration(new Date(0), new Date(d * 60000)) }}</option>
                        </select>
                        <FormLabel :label="$t('duration')" />
                    </div>
                    <div class="col-6 col-md-2">
                        <div class="input-group">
                            <span class="input-group-text"><i class="bi bi-people"></i></span>
                            <input type="number" min="1" v-model.lazy.number="people" class="form-control" />
                        </div>
                        <FormLabel :label="$t('people')" />
                    </div>
                    <div class="col-md-4">
                        <select v-model="buildingId" class="form-select">
                            <option :value="undefined">{{ $t("allBuildings") }}</option>
                            <option v-for="b in buildings" :key="b.id" :value="b.id">{{ b.title }} ({{ b.city }})</option>
                        </select>
                        <FormLabel :label="$t('building')" />
                    </div>
                </div>
                <div class="mt-2">
                    <EquipmentToggles v-model="equipmentId" size="sm" />
                </div>
            </div>
        </div>

        <div class="d-flex flex-wrap align-items-center gap-3 mb-3">
            <span>
                <strong class="text-success">{{ availableCount }}</strong> {{ $t("availableOf") }} {{ matching.length }} {{ $t("matchingRooms") }}
                <small class="text-muted ms-1">{{ formatRange(slotStart, slotEnd) }}</small>
            </span>
            <div class="form-check form-switch mb-0 ms-auto">
                <input id="finder-available" v-model="onlyAvailable" type="checkbox" class="form-check-input" />
                <label for="finder-available" class="form-check-label">{{ $t("onlyAvailable") }}</label>
            </div>
        </div>

        <Feedback :feedback="feedback" />
        <LoadingContainer :is-loading="isLoading">
            <div class="row row-cols-1 row-cols-md-2 row-cols-xl-3 g-3">
                <div v-for="{ room, available } in cards" :key="room.id" class="col">
                    <div class="card room-card h-100" :class="{ 'is-unavailable': !available }" :style="{ borderTopColor: room.$color }">
                        <div class="card-body pb-2">
                            <div class="d-flex align-items-start gap-2">
                                <div class="flex-fill min-w-0">
                                    <h5 class="card-title mb-0 text-truncate">
                                        <RouterLink :to="{ name: 'RoomDetails', params: { id: room.id } }" class="text-decoration-none text-reset">{{ room.title }}</RouterLink>
                                        <small class="text-muted fs-6 ms-1">{{ room.code }}</small>
                                    </h5>
                                    <div class="small text-muted text-truncate"><i class="bi bi-geo-alt me-1"></i>{{ room.$location }}</div>
                                </div>
                                <span class="badge rounded-pill" :class="available ? 'text-bg-success' : 'text-bg-danger'">
                                    <i :class="available ? 'bi bi-check-circle' : 'bi bi-x-circle'" class="me-1"></i>{{ available ? $t("available") : $t("booked") }}
                                </span>
                            </div>
                            <div class="d-flex flex-wrap gap-2 my-2 small">
                                <span class="badge text-bg-light border"><i class="bi bi-people me-1"></i>{{ room.capacity }}</span>
                                <span v-if="room.requiresApproval" class="badge text-bg-warning"><i class="bi bi-shield-lock me-1"></i>{{ $t("requiresApproval") }}</span>
                                <span v-for="eq in room.equipment" :key="eq.id" class="badge text-bg-light border fw-normal" :title="eq.equipment?.description">
                                    <i :class="`bi bi-${eq.equipment?.icon || 'tools'}`" class="me-1 text-primary"></i>{{ eq.equipment?.title }}<template v-if="eq.quantity > 1"> ×{{ eq.quantity }}</template>
                                </span>
                            </div>
                            <DayTimeline :rooms="[room]" :reservations="dayBookings" :day="slotStart" :highlight="{ start: slotStart, end: slotEnd }" :show-room-info="false" compact @select-slot="(s) => { start = toLocalInput(s.start) }" @open="(r) => router.push({ name: 'ReservationDetails', params: { id: r.id } })" />
                        </div>
                        <div class="card-footer bg-transparent d-flex justify-content-between align-items-center">
                            <small class="text-muted">{{ room.description }}</small>
                            <button type="button" class="btn btn-sm text-nowrap" :class="available ? 'btn-primary' : 'btn-outline-secondary'" :disabled="!available" @click="book(room)">
                                <i class="bi bi-calendar-plus me-1"></i>{{ room.requiresApproval ? $t("request") : $t("book") }}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
            <p v-if="!cards.length" class="italic-muted">{{ $t("noRooms") }}</p>
        </LoadingContainer>
    </section>
</template>

<style scoped>
.room-card {
    border-top: 4px solid;
    transition: box-shadow 0.15s ease;
}
.room-card:hover {
    box-shadow: 0 0.25rem 0.75rem rgba(0, 0, 0, 0.1);
}
.room-card.is-unavailable {
    opacity: 0.7;
}
.min-w-0 {
    min-width: 0;
}
</style>
