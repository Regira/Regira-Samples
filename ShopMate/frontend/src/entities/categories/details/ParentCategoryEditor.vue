<!--
    Owned m2m editor for Category.ParentEntities (self-referencing join RelatedCategory).
    Removing a persisted chip marks `_deleted` (undoable until save); EntityService.prepareItem drops them so
    the API's Related() sync deletes by omission. Self-relation: this slice's own components are deep-imported.
-->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import type Entity from "../data/Entity"
import type { RelatedCategory } from "../data/Entity"
import useEntityStore from "../data/store"
import CategoryButton from "./FormModalButton.vue"
import CategorySelector from "../selecting/InputSelector.vue"

const model = defineModel<Array<RelatedCategory>>()
const props = defineProps<{ childId?: number; readonly?: boolean }>()

const { fromPool } = useEntityStore()
const hydrate = (x?: Partial<Entity>) => (x ? fromPool(x as Entity) : undefined)

function exclusions(exclude: Array<number>) {
    // a category can't be its own parent
    return props.childId ? [...exclude, props.childId] : exclude
}
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.parentId" :exclude-key="(r) => r.parentId">
        <template #chip="{ row }">
            <CategoryButton :model-value="hydrate(row.parent)" />
            {{ hydrate(row.parent)?.$label }}
        </template>
        <template #selector="{ add, exclude }">
            <CategorySelector
                v-if="!readonly"
                :filter-defaults="{ exclude: exclusions(exclude) }"
                :placeholder="$t('addParentCategory')"
                @select="(x?: Entity) => x && add({ parentId: x.id, childId: childId ?? 0, parent: x })"
            />
        </template>
    </InputSelectorInline>
</template>
