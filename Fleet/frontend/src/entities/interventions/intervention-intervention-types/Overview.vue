<!-- Owned m2m editor for Intervention.InterventionTypes — each join row is a chip selecting one InterventionType.
     Bind it in the parent Form.vue to the ARRAY: <InterventionInterventionTypeOverview v-model="item.interventionTypes" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as InterventionTypeSelector,
    FormModalButton as InterventionTypeButton,
    useEntityStore as useInterventionTypeStore,
    type Entity as InterventionType,
} from "@/entities/intervention-types"
import type { InterventionInterventionType } from "./Entity"

const model = defineModel<Array<InterventionInterventionType>>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.interventionType.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useInterventionTypeStore()
const hydrate = (x?: Partial<InterventionType>) => fromPool(x as InterventionType)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.interventionTypeId" :exclude-key="(r) => r.interventionTypeId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <InterventionTypeButton :modelValue="hydrate(row.interventionType)" />
            {{ hydrate(row.interventionType)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <InterventionTypeSelector :filter-defaults="{ exclude }" @select="(x?: InterventionType) => x && add({ interventionTypeId: x.id!, interventionType: x })" />
        </template>
    </InputSelectorInline>
</template>
