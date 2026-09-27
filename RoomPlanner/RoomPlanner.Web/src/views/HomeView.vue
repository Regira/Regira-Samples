<script setup lang="ts">
import { onMounted, ref } from "vue"
import { RouterLink } from "vue-router"
import { useConfig } from "@/app-config"
import { Dashboard } from "@/components/entity-navigation"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import { useEntityStore as useRoomStore } from "@/entities/rooms"
import StatusBadge from "@/components/planning/StatusBadge.vue"
import { addDays, formatTime, startOfDay, ReservationStatus } from "@/utilities/planning"

const { title } = useConfig()
const { service: reservationService } = useReservationStore()
const { service: roomService } = useRoomStore()

const today = startOfDay(new Date())
const todayCount = ref<number>()
const pendingCount = ref<number>()
const roomsTotal = ref<number>()
const roomsFreeNow = ref<number>()
const next = ref<Array<Reservation>>([])

onMounted(async () => {
    const now = new Date()
    const live = [ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.PartiallyApproved]
    const [todayResult, pendingResult, allRooms, freeRooms, upcoming] = await Promise.all([
        reservationService.search({ from: today, to: addDays(today, 1), status: live, pageSize: 1 }),
        reservationService.search({ awaitingApproval: true, from: now, pageSize: 1 }),
        roomService.search({ isActive: true, pageSize: 1 }),
        roomService.search({ isActive: true, availableFrom: now, availableTo: new Date(now.getTime() + 60_000), pageSize: 1 }),
        reservationService.search({ from: now, to: addDays(today, 1), status: live, includes: ["Rooms"], sortBy: ["Start"], pageSize: 6 }),
    ])
    todayCount.value = todayResult.count
    pendingCount.value = pendingResult.count
    roomsTotal.value = allRooms.count
    roomsFreeNow.value = freeRooms.count
    next.value = upcoming.items
})

const tiles = [
    { name: "planner", icon: "bi bi-calendar3-range", title: "planner", text: "plannerTile" },
    { name: "roomFinder", icon: "bi bi-search", title: "findARoom", text: "finderTile" },
    { name: "calendar", icon: "bi bi-calendar-week", title: "calendar", text: "calendarTile" },
    { name: "approvals", icon: "bi bi-shield-check", title: "approvals", text: "approvalsTile" },
]
</script>

<template>
    <section>
        <div class="hero text-center py-4 mb-4">
            <h1 class="display-6 fw-semibold mb-1"><i class="bi bi-door-open text-primary me-2"></i>{{ $tm(title) }}</h1>
            <p class="text-muted mb-3">{{ $t("tagline") }}</p>
            <RouterLink :to="{ name: 'ReservationDetails', params: { id: 'new' } }" class="btn btn-primary me-2"><i class="bi bi-calendar-plus me-1"></i>{{ $t("newReservation") }}</RouterLink>
            <RouterLink :to="{ name: 'roomFinder' }" class="btn btn-outline-primary"><i class="bi bi-search me-1"></i>{{ $t("findARoom") }}</RouterLink>
        </div>

        <div class="row g-3 mb-4">
            <div class="col-6 col-lg-3">
                <div class="card stat h-100"><div class="card-body">
                    <div class="text-muted small">{{ $t("bookingsToday") }}</div>
                    <div class="fs-3 fw-semibold">{{ todayCount ?? "…" }}</div>
                </div></div>
            </div>
            <div class="col-6 col-lg-3">
                <RouterLink :to="{ name: 'approvals' }" class="card stat h-100 text-decoration-none"><div class="card-body">
                    <div class="text-muted small">{{ $t("awaitingApproval") }}</div>
                    <div class="fs-3 fw-semibold text-warning">{{ pendingCount ?? "…" }}</div>
                </div></RouterLink>
            </div>
            <div class="col-6 col-lg-3">
                <div class="card stat h-100"><div class="card-body">
                    <div class="text-muted small">{{ $t("roomsFreeNow") }}</div>
                    <div class="fs-3 fw-semibold text-success">{{ roomsFreeNow ?? "…" }} <small class="fs-6 text-muted">/ {{ roomsTotal ?? "…" }}</small></div>
                </div></div>
            </div>
            <div class="col-6 col-lg-3">
                <div class="card stat h-100"><div class="card-body">
                    <div class="text-muted small">{{ $t("occupiedNow") }}</div>
                    <div class="fs-3 fw-semibold">
                        {{ roomsTotal != null && roomsFreeNow != null && roomsTotal > 0 ? Math.round(((roomsTotal - roomsFreeNow) / roomsTotal) * 100) : "…" }}%
                    </div>
                </div></div>
            </div>
        </div>

        <div class="row g-4 mb-4">
            <div class="col-lg-7">
                <div class="row row-cols-1 row-cols-sm-2 g-3">
                    <div v-for="t in tiles" :key="t.name" class="col">
                        <RouterLink :to="{ name: t.name }" class="card tile h-100 text-decoration-none text-reset">
                            <div class="card-body d-flex gap-3 align-items-start">
                                <i :class="t.icon" class="fs-2 text-primary"></i>
                                <div>
                                    <div class="fw-semibold">{{ $t(t.title) }}</div>
                                    <div class="small text-muted">{{ $t(t.text) }}</div>
                                </div>
                            </div>
                        </RouterLink>
                    </div>
                </div>
            </div>
            <div class="col-lg-5">
                <div class="card h-100">
                    <div class="card-header fw-semibold"><i class="bi bi-clock-history me-1"></i>{{ $t("comingUpToday") }}</div>
                    <ul class="list-group list-group-flush">
                        <li v-for="r in next" :key="r.id" class="list-group-item d-flex align-items-center gap-2">
                            <span class="small text-muted text-nowrap">{{ formatTime(r.start) }}–{{ formatTime(r.end) }}</span>
                            <RouterLink :to="{ name: 'ReservationDetails', params: { id: r.id } }" class="text-truncate">{{ r.title }}</RouterLink>
                            <small class="text-muted text-truncate d-none d-sm-inline">{{ r.rooms?.map((x) => x.room?.title).join(", ") }}</small>
                            <StatusBadge class="ms-auto" :status="r.status" compact />
                        </li>
                        <li v-if="!next.length" class="list-group-item italic-muted">{{ $t("nothingPlanned") }}</li>
                    </ul>
                </div>
            </div>
        </div>

        <Dashboard />
    </section>
</template>

<style scoped>
.hero {
    background: linear-gradient(135deg, rgba(var(--bs-primary-rgb), 0.08), rgba(var(--bs-info-rgb), 0.08));
    border-radius: 1rem;
}
.tile,
.stat {
    transition:
        box-shadow 0.15s ease,
        transform 0.15s ease;
}
.tile:hover,
a.stat:hover {
    box-shadow: 0 0.25rem 0.75rem rgba(0, 0, 0, 0.1);
    transform: translateY(-2px);
}
</style>
