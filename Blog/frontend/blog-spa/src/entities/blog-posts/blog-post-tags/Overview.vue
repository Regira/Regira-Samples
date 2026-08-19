<!-- Owned m2m editor for BlogPost.Tags — each join row is a chip selecting one Tag.
     Bind it in the parent Form.vue to the ARRAY: <BlogPostTagOverview v-model="item.tags" />
     Removing a persisted chip marks `_deleted` (tinted, click again to restore); the parent's
     EntityService.prepareItem drops flagged rows so `Related()` deletes them by omission. A chip added this
     session is dropped outright — InputSelectorInline tracks it by identity, so new rows need no id. -->
<script setup lang="ts">
import { InputSelectorInline } from "@regira/modules/vue/entities"
import {
    InputSelector as TagSelector,
    FormModalButton as TagButton,
    useEntityStore as useTagStore,
    type Entity as Tag,
} from "@/entities/tags"
import type { BlogPostTag } from "./Entity"

const model = defineModel<Array<BlogPostTag>>()

// Rows arrive from ?includes= as plain DTOs — they have the API's fields but none of the model's getters, so
// row.tag.$title reads undefined. fromPool rehydrates through the sibling slice's pool, which also
// makes a chip edit relabel live. It is a pass-through, so widen the nested DTO to the entity type here.
const { fromPool } = useTagStore()
const hydrate = (x?: Partial<Tag>) => fromPool(x as Tag)
</script>

<template>
    <InputSelectorInline v-model="model" :row-key="(r) => r.tagId" :exclude-key="(r) => r.tagId">
        <template #chip="{ row }">
            <!-- the related entity's own edit affordance — keep it, a bare label loses the way in -->
            <TagButton :modelValue="hydrate(row.tag)" />
            {{ hydrate(row.tag)?.$title }}
        </template>
        <template #selector="{ add, exclude }">
            <TagSelector :filter-defaults="{ exclude }" @select="(x?: Tag) => x && add({ tagId: x.id!, tag: x })" />
        </template>
    </InputSelectorInline>
</template>
