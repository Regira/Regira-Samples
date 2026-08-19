<!-- Owned m2m editor for Registration.SelectedSessions — each join row is a chip selecting one Session.
     Bind it in the parent Form.vue to the ARRAY: <RegistrationSessionOverview v-model="item.selectedSessions" :event-id="item.eventId" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id.
     `eventId` (added on top of the scaffold default) scopes the picker to sessions of the registration's
     own event — the join has no server-side constraint against picking a session from a different event. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as SessionSelector,
    FormModalButton as SessionButton,
    useEntityStore as useSessionStore,
    type Entity as Session,
} from "@/entities/sessions"
import type { RegistrationSession } from "./Entity"

const model = defineModel<Array<RegistrationSession>>()
defineProps<{ eventId?: number }>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.session.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useSessionStore()
const hydrate = (x?: Partial<Session>) => fromPool(x as Session)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.sessionId" :exclude-key="(r) => r.sessionId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <SessionButton :modelValue="hydrate(row.session)" />
            {{ hydrate(row.session)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <SessionSelector :filter-defaults="{ exclude, eventId }" @select="(x?: Session) => x && add({ sessionId: x.id!, session: x })" />
        </template>
    </InputSelectorInline>
</template>
