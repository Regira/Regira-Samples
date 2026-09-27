<!-- Event card: colour banner (category colour or banner image), date chip, title, place, capacity bar. -->
<template>
    <div class="card ep-event-card h-100" :class="{ 'is-past': event.$isPast, 'is-cancelled': event.status === 'Cancelled' }">
        <div class="ep-event-card-banner" :style="event.$bannerStyle">
            <span class="ep-date-chip" :style="{ '--ep-cat': event.$color }">
                <span class="day">{{ event.$startDay }}</span><span class="month">{{ event.$startMonth }}</span>
            </span>
            <i :class="event.$icon" class="ep-event-card-icon"></i>
            <div class="ep-event-card-badges">
                <span v-if="event.isFeatured" class="badge text-bg-warning"><i class="bi bi-star-fill"></i></span>
                <span v-if="event.status !== 'Published'" class="badge ep-status" :class="`ep-status-${event.status}`">{{ $t(event.status) }}</span>
                <span v-if="event.$dayCount > 1" class="badge text-bg-dark">{{ event.$dayCount }} {{ $t("day").toLowerCase() }}s</span>
            </div>
        </div>
        <div class="card-body d-flex flex-column">
            <div class="small fw-semibold mb-1" :style="{ color: event.$color }">{{ event.category?.title }}</div>
            <h5 class="card-title mb-1">
                <RouterLink :to="{ name: 'EventItemDetails', params: { id: event.$id } }" class="stretched-link text-reset text-decoration-none">
                    {{ event.title }}
                </RouterLink>
            </h5>
            <p v-if="event.summary" class="small text-muted mb-2 ep-clamp-2">{{ event.summary }}</p>
            <div class="small text-muted mt-auto">
                <div class="text-truncate"><i class="bi bi-calendar3 me-1"></i>{{ event.$dateRange }}</div>
                <div class="text-truncate"><i class="bi bi-geo-alt me-1"></i>{{ event.location?.title }}<template v-if="event.location?.city">, {{ event.location.city }}</template></div>
            </div>
            <div v-if="event.maxParticipants" class="mt-2">
                <div class="progress" style="height: 0.35rem">
                    <div class="progress-bar" :style="{ width: event.$fillPercent + '%', background: event.$color }"></div>
                </div>
                <div class="d-flex justify-content-between small mt-1">
                    <span class="text-muted">{{ event.registrationCount ?? 0 }} / {{ event.maxParticipants }}</span>
                    <span v-if="!event.$isPast && event.$spotsLeft === 0" class="text-danger fw-semibold">{{ $t("full") }}</span>
                    <span v-else-if="!event.$isPast" class="fw-semibold" :style="{ color: event.$color }">{{ event.$spotsLeft }} {{ $t("seatsLeft") }}</span>
                </div>
            </div>
        </div>
        <slot name="footer"></slot>
    </div>
</template>

<script setup lang="ts">
import { RouterLink } from "vue-router"
import type Entity from "../data/Entity"

defineProps<{ event: Entity }>()
</script>
