<!-- Loads one day of bookings for the given rooms and renders them on a DayTimeline (used inside the reservation form). -->
<script setup lang="ts">
import { computed, ref, watch } from "vue"
import { useRouter } from "vue-router"
import { LoadingContainer } from "@regira/modules/vue/ui"
import type { Entity as Room } from "@/entities/rooms"
import { useEntityStore as useReservationStore, type Entity as Reservation } from "@/entities/reservations"
import DayTimeline from "./DayTimeline.vue"
import { addDays, startOfDay, ReservationStatus } from "@/utilities/planning"

const props = defineProps<{ rooms: Array<Room>; day: Date; highlight?: { start: Date; end: Date }; excludeId?: number }>()
const emit = defineEmits<{ "select-slot": [{ roomId: number; start: Date }] }>()

const { service } = useReservationStore()
const reservations = ref<Array<Reservation>>([])
const isLoading = ref(false)
const roomIds = computed(() => props.rooms.map((r) => r.id).filter((id) => id > 0))
const dayKey = computed(() => startOfDay(props.day).getTime())

async function load() {
    if (!roomIds.value.length) {
        reservations.value = []
        return
    }
    isLoading.value = true
    try {
        const from = new Date(dayKey.value)
        const { items } = await service.search({
            roomId: roomIds.value,
            from,
            to: addDays(from, 1),
            status: [ReservationStatus.Pending, ReservationStatus.Approved, ReservationStatus.PartiallyApproved],
            includes: ["Rooms"],
            pageSize: 0,
        })
        reservations.value = items
    } finally {
        isLoading.value = false
    }
}
watch([dayKey, () => roomIds.value.join(",")], load, { immediate: true })

const router = useRouter()
const open = (r: Reservation) => window.open(router.resolve({ name: "ReservationDetails", params: { id: r.id } }).href, "_blank")
</script>

<template>
    <LoadingContainer :is-loading="isLoading">
        <DayTimeline
            :rooms="rooms"
            :reservations="reservations"
            :day="day"
            :highlight="highlight"
            :exclude-id="excludeId"
            compact
            @select-slot="emit('select-slot', $event)"
            @open="open"
        />
        <small class="text-muted">{{ $t("timelineHint") }}</small>
    </LoadingContainer>
</template>
