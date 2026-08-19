<!-- Owned m2m editor for Ticket.Categories — each join row is a chip selecting one Category.
     Bind it in the parent Form.vue to the ARRAY: <TicketCategoryOverview v-model="item.categories" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as CategorySelector,
    FormModalButton as CategoryButton,
    useEntityStore as useCategoryStore,
    type Entity as Category,
} from "@/entities/categories"
import type { TicketCategory } from "./Entity"

const model = defineModel<Array<TicketCategory>>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.category.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useCategoryStore()
const hydrate = (x?: Partial<Category>) => fromPool(x as Category)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.categoryId" :exclude-key="(r) => r.categoryId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <CategoryButton :modelValue="hydrate(row.category)" />
            {{ hydrate(row.category)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <CategorySelector :filter-defaults="{ exclude }" @select="(x?: Category) => x && add({ categoryId: x.id!, category: x })" />
        </template>
    </InputSelectorInline>
</template>
