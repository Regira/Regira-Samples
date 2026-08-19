<!-- Owned m2m editor for Session.SessionSpeakers — each join row is a chip selecting one Speaker.
     Bind it in the parent Form.vue to the ARRAY: <SessionSpeakerOverview v-model="item.sessionSpeakers" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as SpeakerSelector,
    FormModalButton as SpeakerButton,
    useEntityStore as useSpeakerStore,
    type Entity as Speaker,
} from "@/entities/speakers"
import type { SessionSpeaker } from "./Entity"

const model = defineModel<Array<SessionSpeaker>>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.speaker.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useSpeakerStore()
const hydrate = (x?: Partial<Speaker>) => fromPool(x as Speaker)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.speakerId" :exclude-key="(r) => r.speakerId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <SpeakerButton :modelValue="hydrate(row.speaker)" />
            {{ hydrate(row.speaker)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <SpeakerSelector :filter-defaults="{ exclude }" @select="(x?: Speaker) => x && add({ speakerId: x.id!, speaker: x })" />
        </template>
    </InputSelectorInline>
</template>
