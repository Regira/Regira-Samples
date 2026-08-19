<!-- Owned m2m editor for Reservation.Rooms — each join row is a chip selecting one MeetingRoom.
     Bind it in the parent Form.vue to the ARRAY: <ReservationRoomOverview v-model="item.rooms" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as MeetingRoomSelector,
    FormModalButton as MeetingRoomButton,
    useEntityStore as useMeetingRoomStore,
    type Entity as MeetingRoom,
} from "@/entities/meeting-rooms"
import type { ReservationRoom } from "./Entity"

const model = defineModel<Array<ReservationRoom>>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.meetingRoom.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useMeetingRoomStore()
const hydrate = (x?: Partial<MeetingRoom>) => fromPool(x as MeetingRoom)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.roomId" :exclude-key="(r) => r.roomId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <MeetingRoomButton :modelValue="hydrate(row.room)" />
            {{ hydrate(row.room)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <MeetingRoomSelector :filter-defaults="{ exclude }" @select="(x?: MeetingRoom) => x && add({ roomId: x.id!, room: x })" />
        </template>
    </InputSelectorInline>
</template>
