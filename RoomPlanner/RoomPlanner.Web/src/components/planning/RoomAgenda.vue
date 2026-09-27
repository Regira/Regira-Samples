<!-- Upcoming bookings of one room (room page, "schedule" tab): a week strip + the list of the next bookings. -->
<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { RouterLink, useRouter } from "vue-router"
import { LoadingContainer, Paging, ResultSummary } from "@regira/modules/vue/ui"
import { PagingInfo } from "@regira/modules/vue/entities"
import type { Entity as Room } from "@/entities/rooms"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import StatusBadge from "./StatusBadge.vue"
import DayTimeline from "./DayTimeline.vue"
import { addDays, formatDay, formatTime, startOfDay, toDateKey } from "@/utilities/planning"

const props = defineProps<{ room: Room }>()
const router = useRouter()
const { service } = useReservationStore()

const today = startOfDay(new Date())
const days = Array.from({ length: 7 }, (_, i) => addDays(today, i))
const weekItems = ref<Array<Reservation>>([])
const items = ref<Array<Reservation>>([])
const count = ref(0)
const pagingInfo = ref(new PagingInfo(10, 1))
const isLoading = ref(false)

async function load() {
    isLoading.value = true
    try {
        const [week, page] = await Promise.all([
            service.search({ roomId: props.room.id, from: today, to: addDays(today, 7), includes: ["Rooms"], pageSize: 0 }),
            service.search({ roomId: props.room.id, from: new Date(), includes: ["Rooms"], sortBy: ["Start"], pageSize: pagingInfo.value.pageSize, page: pagingInfo.value.page }),
        ])
        weekItems.value = week.items
        items.value = page.items
        count.value = page.count ?? 0
    } finally {
        isLoading.value = false
    }
}
watch(() => [props.room.id, pagingInfo.value.page], load, { immediate: true })

const rooms = computed(() => [props.room])
</script>

<template>
    <LoadingContainer :is-loading="isLoading">
        <h6 class="text-muted mb-2"><i class="bi bi-calendar-week me-1"></i>{{ $t("nextSevenDays") }}</h6>
        <div class="mb-4">
            <div v-for="(d, i) in days" :key="d.getTime()" class="d-flex align-items-end">
                <RouterLink :to="{ name: 'planner', query: { day: toDateKey(d), buildingId: room.floor?.buildingId } }" class="small text-nowrap me-2 agenda-day">{{ formatDay(d) }}</RouterLink>
                <div class="flex-fill">
                    <DayTimeline :rooms="rooms" :reservations="weekItems" :day="d" compact :show-room-info="false" :show-header="i === 0" @open="(r) => router.push({ name: 'ReservationDetails', params: { id: r.id } })" @select-slot="(s) => router.push({ name: 'ReservationDetails', params: { id: 'new' }, query: { roomId: room.id, start: s.start.toISOString() } })" />
                </div>
            </div>
        </div>

        <div class="d-flex justify-content-between align-items-center mb-2">
            <h6 class="text-muted mb-0"><i class="bi bi-list-ul me-1"></i>{{ $t("upcomingBookings") }}</h6>
            <ResultSummary v-if="items.length" :visible-count="items.length" :total-count="count" />
        </div>
        <div class="entity-list">
            <div v-for="r in items" :key="r.id" class="row border-bottom py-2 align-items-center">
                <div class="col-4 col-md-3 small">
                    <div>{{ formatDay(r.start) }}</div>
                    <div class="text-muted">{{ formatTime(r.start) }}–{{ formatTime(r.end) }}</div>
                </div>
                <div class="col text-truncate">
                    <RouterLink :to="{ name: 'ReservationDetails', params: { id: r.id } }">{{ r.title }}</RouterLink>
                    <small class="text-muted ms-2 d-none d-md-inline">{{ r.organizer?.title }}</small>
                </div>
                <div class="col-auto"><StatusBadge :status="r.status" compact /></div>
            </div>
        </div>
        <p v-if="!items.length" class="italic-muted">{{ $t("noResults") }}</p>
        <Paging v-if="count > pagingInfo.pageSize!" class="mt-2" v-model="pagingInfo" :count="count" />
    </LoadingContainer>
</template>

<style scoped>
.agenda-day {
    width: 6.5rem;
}
</style>
