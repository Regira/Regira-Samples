<!-- Owned m2m editor for Reservation.Rooms — each join row is a chip selecting one Room, tinted by its approval state.
     Removing a persisted chip marks `_deleted` (click again to restore); the parent's EntityService.prepareItem drops
     flagged rows so `Related()` deletes them by omission. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import { InputSelector as RoomSelector, FormModalButton as RoomButton, useEntityStore as useRoomStore, type Entity as Room } from "@/entities/rooms"
import type { ReservationRoom } from "./Entity"
import { statusVariant, statusIcon } from "@/utilities/planning"

const model = defineModel<Array<ReservationRoom>>()
defineProps<{ readonly?: boolean }>()

const { fromPool } = useRoomStore()
const hydrate = (x?: Partial<Room>) => fromPool(x as Room)
</script>

<template>
    <div v-if="readonly" class="d-flex flex-wrap gap-2">
        <span v-for="row in model" :key="row.roomId" class="badge text-bg-light border fw-normal fs-6 d-inline-flex align-items-center">
            <RoomButton :modelValue="hydrate(row.room)" />
            <span class="room-dot mx-1" :style="{ backgroundColor: hydrate(row.room)?.$color }"></span>{{ hydrate(row.room)?.$title }}
            <i v-if="row.approvalStatus" :class="[statusIcon[row.approvalStatus], `text-${statusVariant[row.approvalStatus]}`]" class="ms-1"></i>
        </span>
    </div>
    <InputSelectorInline v-else v-model="model" :row-key="(r) => r.roomId" :exclude-key="(r) => r.roomId">
        <template #chip="{ row }">
            <RoomButton :modelValue="hydrate(row.room)" />
            <span class="room-dot me-1" :style="{ backgroundColor: hydrate(row.room)?.$color }"></span>
            {{ hydrate(row.room)?.$title }}
            <small class="text-muted ms-1 d-none d-md-inline">{{ hydrate(row.room)?.$location }}</small>
            <i
                v-if="row.approvalStatus"
                :class="[statusIcon[row.approvalStatus], `text-${statusVariant[row.approvalStatus]}`]"
                class="ms-1"
                :title="$t(row.approvalStatus)"
            ></i>
            <i v-else-if="hydrate(row.room)?.requiresApproval" class="bi bi-shield-lock text-warning ms-1" :title="$t('requiresApproval')"></i>
        </template>
        <template #selector="{ add, exclude }">
            <RoomSelector :filter-defaults="{ exclude, isActive: true }" @select="(x?: Room) => x && add({ roomId: x.id!, room: x })" />
        </template>
    </InputSelectorInline>
</template>
