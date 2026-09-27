<template>
    <div class="entity-list sm-card-list">
        <div class="row fw-bold border-bottom py-2 d-none d-md-flex sm-list-head">
            <div class="col-auto"><span class="sm-check invisible"><i class="bi bi-circle"></i></span></div>
            <div class="col">{{ $t("article") }}</div>
            <div class="col-3 d-none d-md-block">{{ $t("shoppingList") }}</div>
            <div class="col-2 d-none d-lg-block">{{ $t("categories") }}</div>
            <div class="col-auto">
                <span v-if="!readonly" class="btn disabled text-muted"><Icon name="delete" /></span>
            </div>
        </div>
        <ListItem
            v-for="(item, i) in items"
            :key="item.$id"
            v-model="items[i]!"
            :readonly="readonly"
            @request-save="$emit('request-save', $event)"
            @request-remove="$emit('request-remove', $event)"
            @save="$emit('save', $event)"
            @remove="$emit('remove', $event)"
        />
    </div>
</template>

<script setup lang="ts">
import { computed } from "vue"
import { Icon } from "@regira/modules/vue/ui"
import type { OverviewEmits } from "@regira/modules/vue/entities"
import type Entity from "../data/Entity"
import useEntityStore from "../data/store"
import ListItem from "./ListItem.vue"

interface Emits extends /* @vue-ignore */ OverviewEmits<Entity> {}
const emit = defineEmits<Emits>()
const props = defineProps<{ modelValue?: Array<Entity>; readonly?: boolean }>()

const { fromPool } = useEntityStore() // resolve rows through the shared pool (reactive cache)
const items = computed<Array<Entity>>({
    get: () => fromPool(props.modelValue || []),
    set: (value) => emit("update:modelValue", value),
})
</script>
